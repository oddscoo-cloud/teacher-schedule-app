namespace backend.Infrastructure.Services;

using backend.Core.Entities;
using backend.Core.Interfaces.Services;
using backend.Data;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using System.Text.Json;

public sealed class HomeworkConflictException : Exception
{
    public HomeworkConflictException(string message) : base(message)
    {
    }
}

public sealed class HomeworkService : IHomeworkService
{
    private readonly AppDbContext _db;
    private readonly StudentWorkspaceAccessService _workspaceAccess;
    private readonly IHomeworkFileStorage _fileStorage;
    private readonly HomeworkFileStorageOptions _fileOptions;

    public HomeworkService(
        AppDbContext db,
        StudentWorkspaceAccessService workspaceAccess,
        IHomeworkFileStorage fileStorage,
        IOptions<HomeworkFileStorageOptions> fileOptions)
    {
        _db = db;
        _workspaceAccess = workspaceAccess;
        _fileStorage = fileStorage;
        _fileOptions = fileOptions.Value;
    }

    public async Task<IReadOnlyList<HomeworkEntity>?> GetHomeworksAsync(
        int userId,
        UserRole role,
        int? membershipId,
        int? studentId,
        int? courseId,
        bool pendingOnly)
    {
        var query = await GetVisibleHomeworksAsync(userId, role, membershipId);
        if (query == null)
            return null;

        if (studentId.HasValue)
            query = query.Where(homework => homework.StudentId == studentId.Value);
        if (courseId.HasValue)
            query = query.Where(homework => homework.CourseId == courseId.Value);

        var homeworks = await IncludeResponseData(query)
            .OrderByDescending(homework => homework.CreatedAt)
            .ToListAsync();
        return pendingOnly
            ? homeworks.Where(homework =>
                homework.Submissions.OrderByDescending(item => item.AttemptNumber).FirstOrDefault() is
                    { Grade: null }).ToList()
            : homeworks;
    }

    public async Task<HomeworkEntity?> GetHomeworkAsync(
        int userId,
        UserRole role,
        int? membershipId,
        int homeworkId)
    {
        var query = await GetVisibleHomeworksAsync(userId, role, membershipId);
        if (query == null)
            return null;
        return await IncludeResponseData(query).SingleOrDefaultAsync(homework => homework.Id == homeworkId);
    }

    public async Task<HomeworkEntity?> CreateHomeworkAsync(
        int userId,
        UserRole role,
        int membershipId,
        HomeworkEntity homework)
    {
        if (role != UserRole.Teacher)
            return null;

        var access = await _workspaceAccess.ResolveAsync(userId, role, membershipId);
        if (access?.TeacherId == null)
            return null;

        var student = await _workspaceAccess.VisibleStudents(access)
            .SingleOrDefaultAsync(item => item.Id == homework.StudentId && item.Status == AccountStatus.Active);
        if (student == null)
            return null;

        if (access.Type == WorkspaceType.School)
        {
            if (!access.SchoolId.HasValue)
                return null;
            if (homework.CourseId.HasValue)
            {
                var courseIsAssigned = await _db.Set<StudentCourseAssignmentEntity>().AnyAsync(assignment =>
                    assignment.StudentId == student.Id &&
                    assignment.CourseId == homework.CourseId.Value &&
                    assignment.Course.SchoolId == access.SchoolId);
                if (!courseIsAssigned)
                    return null;
            }
            homework.SchoolId = access.SchoolId;
        }
        else if (homework.CourseId.HasValue)
        {
            return null;
        }

        homework.TeacherId = access.TeacherId.Value;
        homework.CreatedAt = DateTime.UtcNow;
        _db.Homeworks.Add(homework);
        await _db.SaveChangesAsync();
        return await IncludeResponseData(_db.Homeworks.AsNoTracking())
            .SingleAsync(item => item.Id == homework.Id);
    }

    public async Task<HomeworkSubmissionEntity?> SubmitAsync(
        int userId,
        UserRole role,
        int homeworkId,
        string? text,
        IReadOnlyCollection<string> links,
        IReadOnlyCollection<IFormFile> files)
    {
        if (role != UserRole.Student ||
            !await _db.UserEntities.AnyAsync(user =>
                user.Id == userId && user.Role == UserRole.Student && user.Status == AccountStatus.Active))
            return null;

        var student = await _db.StudentEntities.SingleOrDefaultAsync(item =>
            item.UserId == userId && item.Status == AccountStatus.Active);
        if (student == null)
            return null;

        var homework = await _db.Homeworks.SingleOrDefaultAsync(item =>
            item.Id == homeworkId && item.StudentId == student.Id);
        if (homework == null)
            return null;

        if (text?.Length > 8000)
            throw new InvalidHomeworkFileException("Submission text must not exceed 8000 characters.");
        var normalizedLinks = ValidateLinks(links);
        if (files.Count > _fileOptions.MaxFilesPerSubmission)
            throw new InvalidHomeworkFileException(
                $"A submission can contain at most {_fileOptions.MaxFilesPerSubmission} images.");
        if (string.IsNullOrWhiteSpace(text) && normalizedLinks.Count == 0 && files.Count == 0)
            throw new InvalidHomeworkFileException("Add text, a link, or at least one image before submitting.");

        var latest = await _db.HomeworkSubmissions
            .Where(item => item.HomeworkId == homeworkId)
            .OrderByDescending(item => item.AttemptNumber)
            .FirstOrDefaultAsync();
        if (latest?.Grade == null && latest != null)
            throw new HomeworkConflictException("This submission is already waiting for review.");

        var submission = new HomeworkSubmissionEntity
        {
            HomeworkId = homework.Id,
            AttemptNumber = (latest?.AttemptNumber ?? 0) + 1,
            Text = string.IsNullOrWhiteSpace(text) ? null : text.Trim(),
            LinksJson = JsonSerializer.Serialize(normalizedLinks),
            SubmittedAt = DateTime.UtcNow
        };
        var storedFiles = new List<StoredHomeworkFile>();
        try
        {
            foreach (var file in files)
            {
                var storedFile = await _fileStorage.SaveAsync(file);
                storedFiles.Add(storedFile);
                submission.Attachments.Add(new HomeworkAttachmentEntity
                {
                    StorageName = storedFile.StorageName,
                    DisplayName = storedFile.DisplayName,
                    ContentType = storedFile.ContentType,
                    SizeBytes = storedFile.SizeBytes
                });
            }

            _db.HomeworkSubmissions.Add(submission);
            await _db.SaveChangesAsync();
            return submission;
        }
        catch (Exception submissionException)
        {
            List<Exception>? cleanupExceptions = null;
            foreach (var storedFile in storedFiles)
            {
                try
                {
                    await _fileStorage.DeleteAsync(storedFile.StorageName);
                }
                catch (Exception cleanupException)
                {
                    cleanupExceptions ??= new List<Exception>();
                    cleanupExceptions.Add(cleanupException);
                }
            }

            if (cleanupExceptions != null)
                throw new AggregateException("The submission failed and one or more uploaded files could not be removed.",
                    new[] { submissionException }.Concat(cleanupExceptions));
            throw;
        }
    }

    public async Task<bool?> GradeAsync(
        int userId,
        UserRole role,
        int? membershipId,
        int submissionId,
        int grade)
    {
        if (grade is < 0 or > 100)
            return false;

        var submission = await _db.HomeworkSubmissions
            .Include(item => item.Homework)
            .SingleOrDefaultAsync(item => item.Id == submissionId);
        if (submission == null ||
            submission.Grade.HasValue ||
            !await CanManageHomeworkAsync(userId, role, membershipId, submission.Homework))
            return null;

        var latestSubmissionId = await _db.HomeworkSubmissions
            .Where(item => item.HomeworkId == submission.HomeworkId)
            .OrderByDescending(item => item.AttemptNumber)
            .Select(item => item.Id)
            .FirstAsync();
        if (latestSubmissionId != submission.Id)
            return null;

        submission.Grade = grade;
        submission.GradedAt = DateTime.UtcNow;
        await _db.SaveChangesAsync();
        return true;
    }

    public async Task<HomeworkAttachmentEntity?> GetAttachmentAsync(
        int userId,
        UserRole role,
        int? membershipId,
        int attachmentId)
    {
        var attachment = await _db.HomeworkAttachments
            .Include(item => item.Submission)
                .ThenInclude(submission => submission.Homework)
            .SingleOrDefaultAsync(item => item.Id == attachmentId);
        if (attachment == null ||
            !await CanViewHomeworkAsync(userId, role, membershipId, attachment.Submission.Homework))
            return null;
        return attachment;
    }

    private async Task<IQueryable<HomeworkEntity>?> GetVisibleHomeworksAsync(
        int userId,
        UserRole role,
        int? membershipId)
    {
        if (role == UserRole.Student)
        {
            var studentId = await _db.StudentEntities
                .Where(student =>
                    student.UserId == userId &&
                    student.Status == AccountStatus.Active &&
                    _db.UserEntities.Any(user =>
                        user.Id == userId &&
                        user.Role == UserRole.Student &&
                        user.Status == AccountStatus.Active))
                .Select(student => (int?)student.Id)
                .SingleOrDefaultAsync();
            return studentId.HasValue
                ? _db.Homeworks.Where(homework => homework.StudentId == studentId.Value)
                : null;
        }

        if (!membershipId.HasValue)
            return null;
        var access = await _workspaceAccess.ResolveAsync(userId, role, membershipId.Value);
        if (access == null)
            return null;

        if (access.Role == UserRole.SchoolAdmin && access.SchoolId.HasValue)
            return _db.Homeworks.Where(homework => homework.SchoolId == access.SchoolId.Value);

        if (access.Role == UserRole.Teacher && access.TeacherId.HasValue)
        {
            var visibleStudents = _workspaceAccess.VisibleStudents(access);
            return _db.Homeworks.Where(homework =>
                homework.TeacherId == access.TeacherId.Value &&
                visibleStudents.Any(student => student.Id == homework.StudentId) &&
                (access.Type == WorkspaceType.School
                    ? homework.SchoolId == access.SchoolId
                    : homework.SchoolId == null));
        }

        return null;
    }

    private async Task<bool> CanManageHomeworkAsync(
        int userId,
        UserRole role,
        int? membershipId,
        HomeworkEntity homework)
    {
        var query = await GetVisibleHomeworksAsync(userId, role, membershipId);
        return role != UserRole.Student &&
               query != null &&
               await query.AnyAsync(item => item.Id == homework.Id);
    }

    private async Task<bool> CanViewHomeworkAsync(
        int userId,
        UserRole role,
        int? membershipId,
        HomeworkEntity homework)
    {
        if (role == UserRole.Student)
        {
            return await _db.StudentEntities.AnyAsync(student =>
                student.Id == homework.StudentId &&
                student.UserId == userId &&
                student.Status == AccountStatus.Active);
        }

        return await CanManageHomeworkAsync(userId, role, membershipId, homework);
    }

    private static IQueryable<HomeworkEntity> IncludeResponseData(IQueryable<HomeworkEntity> query)
    {
        return query
            .AsNoTracking()
            .Include(item => item.Student)
            .Include(item => item.Teacher)
            .Include(item => item.Course)
            .Include(item => item.Submissions)
                .ThenInclude(submission => submission.Attachments);
    }

    private static List<string> ValidateLinks(IReadOnlyCollection<string> links)
    {
        if (links.Count > 10)
            throw new InvalidHomeworkFileException("A submission can contain at most 10 links.");

        var normalized = new List<string>();
        foreach (var link in links)
        {
            if (string.IsNullOrWhiteSpace(link) || link.Length > 2048 ||
                !Uri.TryCreate(link.Trim(), UriKind.Absolute, out var uri) ||
                (uri.Scheme != Uri.UriSchemeHttp && uri.Scheme != Uri.UriSchemeHttps))
                throw new InvalidHomeworkFileException("Each link must be a valid HTTP or HTTPS URL.");
            normalized.Add(uri.AbsoluteUri);
        }
        return normalized;
    }
}
