namespace backend.Infrastructure.Services;

using backend.Core.Entities;
using backend.Data;
using Microsoft.EntityFrameworkCore;

public sealed class LessonValidationException(string message) : Exception(message) { }
public sealed class LessonConflictException(string message) : Exception(message) { }
public sealed record LessonTeacherOption(int Id, string FirstName, string LastName);

public sealed class LessonService
{
    private const int MaximumSeriesWeeks = 520;
    private readonly AppDbContext _db;
    private readonly StudentWorkspaceAccessService _workspaceAccess;
    private readonly StudentFinanceService _finance;
    private readonly TimeProvider _clock;

    public LessonService(
        AppDbContext db,
        StudentWorkspaceAccessService workspaceAccess,
        StudentFinanceService finance,
        TimeProvider clock)
    {
        _db = db;
        _workspaceAccess = workspaceAccess;
        _finance = finance;
        _clock = clock;
    }

    public async Task<IReadOnlyList<LessonEntity>?> GetLessonsAsync(
        int userId, UserRole role, int membershipId, DateTimeOffset from, DateTimeOffset to,
        int? studentId = null, int? courseId = null, LessonStatus? status = null)
    {
        if (to <= from || to - from > TimeSpan.FromDays(366))
            throw new LessonValidationException("A valid date range of at most 366 days is required.");
        var access = await _workspaceAccess.ResolveAsync(userId, role, membershipId);
        if (access == null || role == UserRole.Student)
            return null;

        var activeSeries = await VisibleSeries(access).Where(item => item.IsActive).ToListAsync();
        foreach (var series in activeSeries)
        {
            var zone = FindZone(series.Timezone);
            var localFrom = TimeZoneInfo.ConvertTime(from, zone).Date.AddDays(-1);
            var localTo = TimeZoneInfo.ConvertTime(to, zone).Date.AddDays(1);
            await GenerateInstancesAsync(series, localFrom, localTo);
        }
        await _db.SaveChangesAsync();

        var query = VisibleLessons(access);
        query = query.Where(item => item.ScheduledAtUtc >= from.UtcDateTime &&
                                    item.ScheduledAtUtc < to.UtcDateTime);
        if (studentId.HasValue)
            query = query.Where(item => item.StudentId == studentId.Value);
        if (courseId.HasValue)
            query = query.Where(item => item.CourseId == courseId.Value);
        if (status.HasValue)
            query = query.Where(item => item.Status == status.Value);

        return await IncludeLessonData(query)
            .OrderBy(item => item.ScheduledAtUtc)
            .ThenBy(item => item.Id)
            .ToListAsync();
    }

    public async Task<LessonEntity?> GetLessonAsync(int userId, UserRole role, int membershipId, int lessonId)
    {
        var access = await _workspaceAccess.ResolveAsync(userId, role, membershipId);
        if (access == null || role == UserRole.Student)
            return null;
        return await IncludeLessonData(VisibleLessons(access).Where(item => item.Id == lessonId))
            .SingleOrDefaultAsync();
    }

    public Task<int> CountSeriesInstancesAsync(int seriesId) =>
        _db.LessonEvents.CountAsync(item => item.SeriesId == seriesId);

    public async Task<LessonSeriesEntity?> GetSeriesAsync(
        int userId, UserRole role, int membershipId, int seriesId)
    {
        var access = await _workspaceAccess.ResolveAsync(userId, role, membershipId);
        if (access == null || role == UserRole.Student)
            return null;
        return await VisibleSeries(access).SingleOrDefaultAsync(item => item.Id == seriesId);
    }

    public async Task<IReadOnlyList<LessonTeacherOption>?> GetTeachersAsync(
        int userId, UserRole role, int membershipId)
    {
        var access = await _workspaceAccess.ResolveAsync(userId, role, membershipId);
        if (access == null || role == UserRole.Student)
            return null;
        var teachers = _db.TeacherEntities.AsNoTracking();
        if (access.Type == WorkspaceType.School && access.SchoolId.HasValue)
            teachers = teachers.Where(item => item.SchoolId == access.SchoolId);
        else
            teachers = teachers.Where(item => item.Id == access.TeacherId);
        return await teachers.OrderBy(item => item.LastName).ThenBy(item => item.FirstName)
            .Select(item => new LessonTeacherOption(item.Id, item.FirstName, item.LastName))
            .ToListAsync();
    }

    public async Task<LessonEntity?> CreateLessonAsync(
        int userId, UserRole role, int membershipId, int studentId, int? requestedTeacherId,
        int? courseId, string title, string? description, DateTimeOffset scheduledAt,
        int durationMinutes, string timezone)
    {
        ValidateCommon(title, description, durationMinutes, timezone);
        ValidateZone(timezone);
        var access = await ResolveWritableAccessAsync(userId, role, membershipId, studentId, requestedTeacherId, courseId);
        if (access == null)
            return null;

        var now = _clock.GetUtcNow().UtcDateTime;
        var lesson = new LessonEntity
        {
            TeacherId = access.TeacherId!.Value,
            StudentId = studentId,
            SchoolId = access.SchoolId,
            CourseId = courseId,
            ScheduledAtUtc = scheduledAt.UtcDateTime,
            Title = title.Trim(),
            Description = Normalize(description),
            Duration = TimeSpan.FromMinutes(durationMinutes),
            Timezone = timezone,
            CreatedAtUtc = now,
            ModifiedAtUtc = now,
            ModifiedByUserId = userId
        };
        _db.LessonEvents.Add(lesson);
        await _db.SaveChangesAsync();
        return await IncludeLessonData(_db.LessonEvents.Where(item => item.Id == lesson.Id)).SingleAsync();
    }

    public async Task<LessonSeriesEntity?> CreateSeriesAsync(
        int userId, UserRole role, int membershipId, int studentId, int? requestedTeacherId, int? courseId,
        DayOfWeek dayOfWeek, TimeSpan startTime, int durationMinutes, string title, string? description,
        DateTime startDate, DateTime? endDate, string timezone)
    {
        ValidateCommon(title, description, durationMinutes, timezone);
        ValidateZone(timezone);
        if (!Enum.IsDefined(dayOfWeek) || startTime < TimeSpan.Zero || startTime >= TimeSpan.FromDays(1) ||
            startTime.Ticks % TimeSpan.TicksPerMinute != 0)
            throw new LessonValidationException("A valid weekday and local start time (minute precision) are required.");
        var localStart = DateTime.SpecifyKind(startDate.Date, DateTimeKind.Unspecified);
        DateTime? localEnd = endDate.HasValue ? DateTime.SpecifyKind(endDate.Value.Date, DateTimeKind.Unspecified) : null;
        if (localEnd.HasValue && localEnd.Value < localStart)
            throw new LessonValidationException("Series end date cannot be before its start date.");
        if (localEnd.HasValue && localEnd.Value > localStart.AddDays(7 * MaximumSeriesWeeks))
            throw new LessonValidationException("A lesson series cannot span more than ten years.");

        var access = await ResolveWritableAccessAsync(userId, role, membershipId, studentId, requestedTeacherId, courseId);
        if (access == null)
            return null;

        var series = new LessonSeriesEntity
        {
            TeacherId = access.TeacherId!.Value,
            StudentId = studentId,
            SchoolId = access.SchoolId,
            CourseId = courseId,
            DayOfWeek = dayOfWeek,
            StartTime = startTime,
            Duration = TimeSpan.FromMinutes(durationMinutes),
            Title = title.Trim(),
            Description = Normalize(description),
            StartDate = localStart,
            EndDate = localEnd,
            Timezone = timezone,
            CreatedAt = _clock.GetUtcNow().UtcDateTime,
            IsActive = true
        };
        _db.LessonSeries.Add(series);
        await _db.SaveChangesAsync();
        await GenerateInstancesAsync(series, localStart, localEnd ?? localStart.AddDays(364));
        await _db.SaveChangesAsync();
        return series;
    }

    public async Task<LessonEntity?> UpdateLessonAsync(
        int userId, UserRole role, int membershipId, int lessonId, string title, string? description,
        int? courseId, DateTimeOffset scheduledAt, int durationMinutes, string timezone, string? notes)
    {
        ValidateCommon(title, description, durationMinutes, timezone);
        ValidateZone(timezone);
        var access = await _workspaceAccess.ResolveAsync(userId, role, membershipId);
        if (access == null)
            return null;
        var lesson = await IncludeLessonData(VisibleLessons(access).Where(item => item.Id == lessonId))
            .SingleOrDefaultAsync();
        if (lesson == null)
            return null;
        if (lesson.Status != LessonStatus.Scheduled)
            throw new LessonConflictException("Only scheduled lessons can be edited.");
        if (lesson.CourseId != courseId)
        {
            var courseAccess = await ResolveWritableAccessAsync(
                userId, role, membershipId, lesson.StudentId, lesson.TeacherId, courseId);
            if (courseAccess == null)
                return null;
        }
        lesson.Title = title.Trim();
        lesson.Description = Normalize(description);
        lesson.CourseId = courseId;
        lesson.ScheduledAtUtc = scheduledAt.UtcDateTime;
        lesson.Duration = TimeSpan.FromMinutes(durationMinutes);
        lesson.Timezone = timezone;
        lesson.Notes = Normalize(notes);
        lesson.ModifiedAtUtc = _clock.GetUtcNow().UtcDateTime;
        lesson.ModifiedByUserId = userId;
        await _db.SaveChangesAsync();
        return lesson;
    }

    public async Task<bool?> UpdateSeriesAsync(
        int userId, UserRole role, int membershipId, int seriesId, DayOfWeek dayOfWeek, TimeSpan startTime,
        int durationMinutes, string title, string? description, string timezone)
    {
        ValidateCommon(title, description, durationMinutes, timezone);
        ValidateZone(timezone);
        if (!Enum.IsDefined(dayOfWeek) || startTime < TimeSpan.Zero || startTime >= TimeSpan.FromDays(1) ||
            startTime.Ticks % TimeSpan.TicksPerMinute != 0)
            throw new LessonValidationException("A valid weekday and local start time (minute precision) are required.");
        var access = await _workspaceAccess.ResolveAsync(userId, role, membershipId);
        if (access == null)
            return null;
        var seriesQuery = VisibleSeries(access).Where(item => item.Id == seriesId);
        var series = await seriesQuery.SingleOrDefaultAsync();
        if (series == null)
            return null;
        if (!series.IsActive)
            throw new LessonConflictException("This lesson series is no longer active.");

        series.DayOfWeek = dayOfWeek;
        series.StartTime = startTime;
        series.Duration = TimeSpan.FromMinutes(durationMinutes);
        series.Title = title.Trim();
        series.Description = Normalize(description);
        series.Timezone = timezone;

        var now = _clock.GetUtcNow().UtcDateTime;
        var futureLessons = await _db.LessonEvents
            .Where(item => item.SeriesId == seriesId && item.Status == LessonStatus.Scheduled &&
                           item.ScheduledAtUtc >= now)
            .ToListAsync();
        foreach (var lesson in futureLessons)
        {
            if (!lesson.SeriesInstanceIndex.HasValue)
                continue;
            var localDate = GetOccurrenceDate(series, lesson.SeriesInstanceIndex.Value);
            lesson.ScheduledAtUtc = LocalToUtc(localDate.Add(series.StartTime), timezone);
            lesson.Duration = series.Duration;
            lesson.Title = series.Title;
            lesson.Description = series.Description;
            lesson.Timezone = timezone;
            lesson.ModifiedAtUtc = now;
            lesson.ModifiedByUserId = userId;
        }
        await _db.SaveChangesAsync();
        return true;
    }

    public async Task<bool?> ChangeStatusAsync(
        int userId, UserRole role, int membershipId, int lessonId, LessonStatus status, string? reason = null)
    {
        if (status is not (LessonStatus.Completed or LessonStatus.Cancelled))
            throw new LessonValidationException("Only Completed or Cancelled status can be set directly.");
        if (reason?.Length > 500)
            throw new LessonValidationException("Status reason cannot exceed 500 characters.");
        var access = await _workspaceAccess.ResolveAsync(userId, role, membershipId);
        if (access == null)
            return null;
        var lesson = await VisibleLessons(access).SingleOrDefaultAsync(item => item.Id == lessonId);
        if (lesson == null)
            return null;
        if (lesson.Status != LessonStatus.Scheduled)
        {
            if (lesson.Status == status)
                return true;
            throw new LessonConflictException("Only scheduled lessons can change status.");
        }
        if (status == LessonStatus.Completed && lesson.ScheduledAtUtc > _clock.GetUtcNow().UtcDateTime)
            throw new LessonConflictException("A lesson cannot be completed before it starts.");

        var previousStatus = lesson.Status;
        lesson.Status = status;
        lesson.ModifiedAtUtc = _clock.GetUtcNow().UtcDateTime;
        lesson.ModifiedByUserId = userId;
        lesson.StatusReason = Normalize(reason);
        var chargeType = status == LessonStatus.Completed
            ? StudentLessonChargeType.Completed
            : StudentLessonChargeType.LateCancellation;
        var charge = await _finance.RecordLessonChargeAsync(
            userId, role, membershipId, lesson.StudentId, lesson.Id, chargeType,
            new DateTimeOffset(DateTime.SpecifyKind(lesson.ScheduledAtUtc, DateTimeKind.Utc)),
            status == LessonStatus.Cancelled
                ? new DateTimeOffset(DateTime.SpecifyKind(lesson.ModifiedAtUtc, DateTimeKind.Utc))
                : null);
        if (charge.Status is FinanceWriteStatus.NotFound or FinanceWriteStatus.Invalid or FinanceWriteStatus.Conflict)
        {
            lesson.Status = previousStatus;
            _db.Entry(lesson).State = EntityState.Unchanged;
            if (charge.Status == FinanceWriteStatus.Invalid)
                throw new LessonValidationException(charge.Error ?? "The lesson charge could not be recorded.");
            throw new LessonConflictException(charge.Error ?? "The lesson charge conflicts with an existing charge.");
        }

        await _db.SaveChangesAsync();
        return true;
    }

    public async Task<LessonEntity?> RescheduleAsync(
        int userId, UserRole role, int membershipId, int lessonId, DateTimeOffset scheduledAt, string? reason)
    {
        if (reason?.Length > 500)
            throw new LessonValidationException("Reschedule reason cannot exceed 500 characters.");
        var access = await _workspaceAccess.ResolveAsync(userId, role, membershipId);
        if (access == null)
            return null;
        var original = await IncludeLessonData(VisibleLessons(access).Where(item => item.Id == lessonId))
            .SingleOrDefaultAsync();
        if (original == null)
            return null;
        if (original.Status != LessonStatus.Scheduled)
            throw new LessonConflictException("Only scheduled lessons can be rescheduled.");

        var now = _clock.GetUtcNow().UtcDateTime;
        var replacement = new LessonEntity
        {
            TeacherId = original.TeacherId,
            StudentId = original.StudentId,
            SchoolId = original.SchoolId,
            CourseId = original.CourseId,
            ScheduledAtUtc = scheduledAt.UtcDateTime,
            Title = original.Title,
            Description = original.Description,
            Duration = original.Duration,
            Timezone = original.Timezone,
            CreatedAtUtc = now,
            ModifiedAtUtc = now,
            ModifiedByUserId = userId,
            RescheduledFromId = original.Id
        };
        original.Status = LessonStatus.Rescheduled;
        original.ModifiedAtUtc = now;
        original.ModifiedByUserId = userId;
        original.StatusReason = Normalize(reason);
        _db.LessonEvents.Add(replacement);
        _db.LessonReschedules.Add(new LessonRescheduleEntity
        {
            OriginalLesson = original,
            NewLesson = replacement,
            RescheduleReason = Normalize(reason),
            RescheduleDateUtc = now,
            RescheduleByUserId = userId
        });
        try
        {
            await _db.SaveChangesAsync();
        }
        catch (DbUpdateException)
        {
            original.Status = LessonStatus.Scheduled;
            _db.Entry(original).State = EntityState.Unchanged;
            _db.Entry(replacement).State = EntityState.Detached;
            var historyEntry = _db.ChangeTracker.Entries<LessonRescheduleEntity>()
                .SingleOrDefault(item => item.Entity.NewLesson == replacement);
            if (historyEntry != null)
                historyEntry.State = EntityState.Detached;
            throw new LessonConflictException("This lesson has already been rescheduled.");
        }
        return await IncludeLessonData(_db.LessonEvents.Where(item => item.Id == replacement.Id)).SingleAsync();
    }

    private async Task<StudentWorkspaceAccess?> ResolveWritableAccessAsync(
        int userId, UserRole role, int membershipId, int studentId, int? requestedTeacherId, int? courseId)
    {
        var access = await _workspaceAccess.ResolveAsync(userId, role, membershipId);
        if (access == null || role == UserRole.Student)
            return null;
        var student = await _workspaceAccess.VisibleStudents(access)
            .SingleOrDefaultAsync(item => item.Id == studentId && item.Status == AccountStatus.Active);
        if (student == null)
            return null;

        var teacherId = access.TeacherId ?? requestedTeacherId;
        if (!teacherId.HasValue)
            throw new LessonValidationException("A teacher is required for this school calendar.");
        if (access.TeacherId.HasValue && requestedTeacherId.HasValue && requestedTeacherId != access.TeacherId)
            return null;
        var teacher = await _db.TeacherEntities.SingleOrDefaultAsync(item =>
            item.Id == teacherId.Value &&
            (access.Type == WorkspaceType.School
                ? item.SchoolId == access.SchoolId
                : item.SchoolId == null && item.Id == access.TeacherId));
        if (teacher == null)
            return null;

        if (courseId.HasValue)
        {
            var isAssignedCourse = await _db.StudentCourseAssignments.AnyAsync(item =>
                item.StudentId == studentId && item.CourseId == courseId.Value &&
                item.Course.SchoolId == access.SchoolId);
            if (access.Type != WorkspaceType.School || !isAssignedCourse)
                throw new LessonValidationException("The selected course is not assigned to this student in the current workspace.");
        }
        return access with { TeacherId = teacherId };
    }

    private IQueryable<LessonEntity> VisibleLessons(StudentWorkspaceAccess access)
    {
        var query = _db.LessonEvents.AsQueryable();
        if (access.Type == WorkspaceType.School && access.SchoolId.HasValue)
            query = query.Where(item => item.SchoolId == access.SchoolId);
        else
            query = query.Where(item => item.SchoolId == null && item.TeacherId == access.TeacherId);

        if (access.Role == UserRole.Teacher)
            query = query.Where(item => item.TeacherId == access.TeacherId);
        return query.Where(item => _workspaceAccess.VisibleStudents(access).Any(student => student.Id == item.StudentId));
    }

    private IQueryable<LessonSeriesEntity> VisibleSeries(StudentWorkspaceAccess access)
    {
        var query = _db.LessonSeries.AsQueryable();
        if (access.Type == WorkspaceType.School && access.SchoolId.HasValue)
            query = query.Where(item => item.SchoolId == access.SchoolId);
        else
            query = query.Where(item => item.SchoolId == null && item.TeacherId == access.TeacherId);
        if (access.Role == UserRole.Teacher)
            query = query.Where(item => item.TeacherId == access.TeacherId);
        return query.Where(item => _workspaceAccess.VisibleStudents(access).Any(student => student.Id == item.StudentId));
    }

    private static IQueryable<LessonEntity> IncludeLessonData(IQueryable<LessonEntity> query) =>
        query.Include(item => item.Student).Include(item => item.Course)
            .Include(item => item.RescheduleHistoryAsOriginal).ThenInclude(item => item.NewLesson)
            .Include(item => item.RescheduleHistoryAsNew).ThenInclude(item => item.OriginalLesson);

    private async Task GenerateInstancesAsync(LessonSeriesEntity series, DateTime from, DateTime to)
    {
        var existingIndices = await _db.LessonEvents.Where(item => item.SeriesId == series.Id)
            .Select(item => item.SeriesInstanceIndex).ToListAsync();
        var existing = existingIndices.Where(index => index.HasValue).Select(index => index!.Value).ToHashSet();
        for (var index = 0; index < MaximumSeriesWeeks; index++)
        {
            var localDate = GetOccurrenceDate(series, index);
            if (localDate > to || localDate < from)
                continue;
            if (series.EndDate.HasValue && localDate.Date > series.EndDate.Value.Date)
                continue;
            if (existing.Contains(index))
                continue;
            var utc = LocalToUtc(localDate.Add(series.StartTime), series.Timezone);
            var now = _clock.GetUtcNow().UtcDateTime;
            _db.LessonEvents.Add(new LessonEntity
            {
                SeriesId = series.Id,
                TeacherId = series.TeacherId,
                StudentId = series.StudentId,
                SchoolId = series.SchoolId,
                CourseId = series.CourseId,
                ScheduledAtUtc = utc,
                Title = series.Title,
                Description = series.Description,
                Duration = series.Duration,
                Timezone = series.Timezone,
                CreatedAtUtc = now,
                ModifiedAtUtc = now,
                SeriesInstanceIndex = index
            });
        }
    }

    private static DateTime GetOccurrenceDate(LessonSeriesEntity series, int index)
    {
        var date = DateTime.SpecifyKind(series.StartDate.Date, DateTimeKind.Unspecified);
        var delta = ((int)series.DayOfWeek - (int)date.DayOfWeek + 7) % 7;
        return date.AddDays(delta + index * 7);
    }

    private static DateTime LocalToUtc(DateTime local, string timezone)
    {
        var zone = FindZone(timezone);
        local = DateTime.SpecifyKind(local, DateTimeKind.Unspecified);
        if (zone.IsInvalidTime(local))
        {
            var adjusted = local;
            do adjusted = adjusted.AddMinutes(1); while (zone.IsInvalidTime(adjusted));
            local = adjusted;
        }
        var offset = zone.IsAmbiguousTime(local)
            ? zone.GetAmbiguousTimeOffsets(local).Max()
            : zone.GetUtcOffset(local);
        return new DateTimeOffset(local, offset).UtcDateTime;
    }

    private static void ValidateCommon(string title, string? description, int durationMinutes, string timezone)
    {
        if (string.IsNullOrWhiteSpace(title) || title.Trim().Length > 255)
            throw new LessonValidationException("A title of 1 to 255 characters is required.");
        if (description?.Length > 4000)
            throw new LessonValidationException("Description cannot exceed 4000 characters.");
        if (durationMinutes is < 1 or > 1440)
            throw new LessonValidationException("Duration must be between 1 and 1440 minutes.");
        if (string.IsNullOrWhiteSpace(timezone) || timezone.Length > 100)
            throw new LessonValidationException("An explicit time zone is required.");
    }

    private static void ValidateZone(string timezone)
    {
        try { _ = FindZone(timezone); }
        catch (TimeZoneNotFoundException) { throw new LessonValidationException("The specified time zone is not available."); }
        catch (InvalidTimeZoneException) { throw new LessonValidationException("The specified time zone is invalid."); }
    }

    private static TimeZoneInfo FindZone(string timezone) => TimeZoneInfo.FindSystemTimeZoneById(timezone);
    private static string? Normalize(string? value) => string.IsNullOrWhiteSpace(value) ? null : value.Trim();
}
