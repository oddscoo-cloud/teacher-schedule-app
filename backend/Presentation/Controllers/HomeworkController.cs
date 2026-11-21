namespace backend.Presentation.Controllers;

using backend.Core.Entities;
using backend.Core.Interfaces.Services;
using backend.Infrastructure.Services;
using backend.Presentation.DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using System.Text.Json;

[ApiController]
[Authorize]
[Route("api/homework")]
public sealed class HomeworkController : ControllerBase
{
    private readonly IHomeworkService _homework;
    private readonly IHomeworkFileStorage _fileStorage;

    public HomeworkController(IHomeworkService homework, IHomeworkFileStorage fileStorage)
    {
        _homework = homework;
        _fileStorage = fileStorage;
    }

    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<HomeworkResponseDto>>> GetHomeworks(
        [FromQuery] int? studentId,
        [FromQuery] int? courseId,
        [FromQuery] bool pendingOnly = false)
    {
        if (!TryGetRequester(out var userId, out var role, out var membershipId))
            return Unauthorized();

        var homeworks = await _homework.GetHomeworksAsync(
            userId, role, membershipId, studentId, courseId, pendingOnly);
        return homeworks == null
            ? Forbid()
            : Ok(homeworks.Select(MapHomework).ToList());
    }

    [HttpGet("{homeworkId:int}")]
    public async Task<ActionResult<HomeworkResponseDto>> GetHomework(int homeworkId)
    {
        if (!TryGetRequester(out var userId, out var role, out var membershipId))
            return Unauthorized();

        var homework = await _homework.GetHomeworkAsync(userId, role, membershipId, homeworkId);
        return homework == null ? NotFound() : Ok(MapHomework(homework));
    }

    [HttpPost]
    public async Task<ActionResult<HomeworkResponseDto>> CreateHomework(
        [FromBody] HomeworkCreateRequestDto request)
    {
        if (!TryGetRequester(out var userId, out var role, out var membershipId))
            return Unauthorized();
        if (role != UserRole.Teacher || !membershipId.HasValue)
            return Forbid();

        var created = await _homework.CreateHomeworkAsync(userId, role, membershipId.Value, new HomeworkEntity
        {
            StudentId = request.StudentId,
            CourseId = request.CourseId,
            Title = request.Title.Trim(),
            Instructions = string.IsNullOrWhiteSpace(request.Instructions) ? null : request.Instructions.Trim(),
            DueAt = request.DueAt
        });
        return created == null
            ? Forbid()
            : CreatedAtAction(nameof(GetHomework), new { homeworkId = created.Id }, MapHomework(created));
    }

    [HttpPost("{homeworkId:int}/submissions")]
    [RequestSizeLimit(30_000_000)]
    [RequestFormLimits(MultipartBodyLengthLimit = 30_000_000)]
    public async Task<ActionResult<HomeworkSubmissionResponseDto>> Submit(
        int homeworkId,
        [FromForm] HomeworkSubmissionRequest request)
    {
        if (!TryGetRequester(out var userId, out var role, out _))
            return Unauthorized();

        List<string>? links;
        try
        {
            links = JsonSerializer.Deserialize<List<string>>(request.LinksJson);
        }
        catch (JsonException)
        {
            return BadRequest(new { message = "Links must be a JSON array of URLs." });
        }
        if (links == null)
            return BadRequest(new { message = "Links must be a JSON array of URLs." });

        try
        {
            var submission = await _homework.SubmitAsync(
                userId, role, homeworkId, request.Text, links, request.Files);
            return submission == null
                ? NotFound()
                : Ok(MapSubmission(submission));
        }
        catch (InvalidHomeworkFileException exception)
        {
            return BadRequest(new { message = exception.Message });
        }
        catch (HomeworkConflictException exception)
        {
            return Conflict(new { message = exception.Message });
        }
    }

    [HttpPost("submissions/{submissionId:int}/grade")]
    public async Task<IActionResult> Grade(int submissionId, [FromBody] HomeworkGradeRequestDto request)
    {
        if (!TryGetRequester(out var userId, out var role, out var membershipId))
            return Unauthorized();
        if (role == UserRole.Student || !membershipId.HasValue)
            return Forbid();

        var graded = await _homework.GradeAsync(userId, role, membershipId, submissionId, request.Grade);
        return graded switch
        {
            true => NoContent(),
            false => Conflict(new { message = "The grade must be between 0 and 100 and the submission must be awaiting review." }),
            null => NotFound()
        };
    }

    [HttpGet("attachments/{attachmentId:int}")]
    public async Task<IActionResult> DownloadAttachment(int attachmentId)
    {
        if (!TryGetRequester(out var userId, out var role, out var membershipId))
            return Unauthorized();

        var attachment = await _homework.GetAttachmentAsync(userId, role, membershipId, attachmentId);
        if (attachment == null)
            return NotFound();

        try
        {
            return File(
                _fileStorage.OpenRead(attachment.StorageName),
                attachment.ContentType,
                attachment.DisplayName,
                enableRangeProcessing: false);
        }
        catch (FileNotFoundException)
        {
            return NotFound();
        }
    }

    private bool TryGetRequester(out int userId, out UserRole role, out int? membershipId)
    {
        userId = 0;
        role = default;
        membershipId = int.TryParse(User.FindFirstValue("workspaceMembershipId"), out var parsedMembership)
            ? parsedMembership
            : null;

        return int.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out userId) &&
               Enum.TryParse(User.FindFirstValue(ClaimTypes.Role), out role);
    }

    private static HomeworkResponseDto MapHomework(HomeworkEntity homework)
    {
        return new HomeworkResponseDto
        {
            Id = homework.Id,
            StudentId = homework.StudentId,
            StudentName = $"{homework.Student.FirstName} {homework.Student.LastName}",
            TeacherId = homework.TeacherId,
            TeacherName = $"{homework.Teacher.FirstName} {homework.Teacher.LastName}",
            CourseId = homework.CourseId,
            CourseName = homework.Course?.Name,
            Title = homework.Title,
            Instructions = homework.Instructions,
            DueAt = homework.DueAt,
            CreatedAt = homework.CreatedAt,
            Submissions = homework.Submissions
                .OrderBy(item => item.AttemptNumber)
                .Select(MapSubmission)
                .ToList()
        };
    }

    private static HomeworkSubmissionResponseDto MapSubmission(HomeworkSubmissionEntity submission)
    {
        List<string> links;
        try
        {
            links = JsonSerializer.Deserialize<List<string>>(submission.LinksJson) ?? new List<string>();
        }
        catch (JsonException)
        {
            links = new List<string>();
        }

        return new HomeworkSubmissionResponseDto
        {
            Id = submission.Id,
            AttemptNumber = submission.AttemptNumber,
            Text = submission.Text,
            Links = links,
            Grade = submission.Grade,
            SubmittedAt = submission.SubmittedAt,
            GradedAt = submission.GradedAt,
            Attachments = submission.Attachments.Select(attachment => new HomeworkAttachmentResponseDto
            {
                Id = attachment.Id,
                DisplayName = attachment.DisplayName,
                ContentType = attachment.ContentType,
                SizeBytes = attachment.SizeBytes,
                DownloadUrl = $"/api/homework/attachments/{attachment.Id}"
            }).ToList()
        };
    }
}

public sealed class HomeworkSubmissionRequest
{
    public string? Text { get; set; }
    public string LinksJson { get; set; } = "[]";
    public List<IFormFile> Files { get; set; } = new();
}
