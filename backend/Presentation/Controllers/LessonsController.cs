namespace backend.Presentation.Controllers;

using backend.Core.Entities;
using backend.Infrastructure.Services;
using backend.Presentation.DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

[ApiController]
[Authorize(Roles = "Teacher,SchoolAdmin")]
[Route("api/lessons")]
public sealed class LessonsController : AuthorizedWorkspaceControllerBase
{
    private readonly LessonService _lessons;

    public LessonsController(LessonService lessons)
    {
        _lessons = lessons;
    }

    [HttpGet("teachers")]
    public async Task<ActionResult<IReadOnlyList<LessonTeacherOption>>> GetTeachers()
    {
        if (!TryGetRequester(out var requester))
            return Unauthorized();
        var teachers = await _lessons.GetTeachersAsync(
            requester.UserId, requester.Role, requester.MembershipId);
        return teachers == null ? Forbid() : Ok(teachers);
    }

    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<LessonResponseDto>>> GetLessons(
        [FromQuery] DateTimeOffset? from,
        [FromQuery] DateTimeOffset? to,
        [FromQuery] int? studentId,
        [FromQuery] int? courseId,
        [FromQuery] string? status)
    {
        if (!TryGetRequester(out var requester))
            return Unauthorized();
        if (!from.HasValue || !to.HasValue)
            return BadRequest(new { message = "Both from and to date-time values are required." });
        LessonStatus? parsedStatus = null;
        if (!string.IsNullOrWhiteSpace(status))
        {
            if (!Enum.TryParse<LessonStatus>(status, true, out var value) || !Enum.IsDefined(value))
                return BadRequest(new { message = "Unknown lesson status." });
            parsedStatus = value;
        }
        try
        {
            var result = await _lessons.GetLessonsAsync(
                requester.UserId, requester.Role, requester.MembershipId,
                from.Value, to.Value, studentId, courseId, parsedStatus);
            return result == null ? Forbid() : Ok(result.Select(MapLesson).ToList());
        }
        catch (LessonValidationException exception)
        {
            return BadRequest(new { message = exception.Message });
        }
    }

    [HttpGet("{lessonId:int}")]
    public async Task<ActionResult<LessonResponseDto>> GetLesson(int lessonId)
    {
        if (!TryGetRequester(out var requester))
            return Unauthorized();
        var lesson = await _lessons.GetLessonAsync(
            requester.UserId, requester.Role, requester.MembershipId, lessonId);
        return lesson == null ? NotFound() : Ok(MapLesson(lesson));
    }

    [HttpGet("series/{seriesId:int}")]
    public async Task<ActionResult<LessonSeriesResponseDto>> GetSeries(int seriesId)
    {
        if (!TryGetRequester(out var requester))
            return Unauthorized();
        var series = await _lessons.GetSeriesAsync(
            requester.UserId, requester.Role, requester.MembershipId, seriesId);
        return series == null ? NotFound() : Ok(await ToSeriesResponse(series));
    }

    [HttpPost]
    public async Task<ActionResult<LessonResponseDto>> CreateLesson(LessonRequestDto request)
    {
        if (!TryGetRequester(out var requester))
            return Unauthorized();
        if (!request.ScheduledAt.HasValue)
            return BadRequest(new { message = "Lesson start date-time is required." });
        try
        {
            var lesson = await _lessons.CreateLessonAsync(
                requester.UserId, requester.Role, requester.MembershipId, request.StudentId,
                request.TeacherId, request.CourseId, request.Title, request.Description,
                request.ScheduledAt.Value, request.DurationMinutes, request.Timezone);
            return lesson == null
                ? NotFound()
                : CreatedAtAction(nameof(GetLesson), new { lessonId = lesson.Id }, MapLesson(lesson));
        }
        catch (LessonValidationException exception)
        {
            return BadRequest(new { message = exception.Message });
        }
    }

    [HttpPost("series")]
    public async Task<ActionResult<LessonSeriesResponseDto>> CreateSeries(LessonSeriesRequestDto request)
    {
        if (!TryGetRequester(out var requester))
            return Unauthorized();
        if (!request.StartDate.HasValue)
            return BadRequest(new { message = "Series start date is required." });
        try
        {
            var series = await _lessons.CreateSeriesAsync(
                requester.UserId, requester.Role, requester.MembershipId, request.StudentId,
                request.TeacherId, request.CourseId, request.DayOfWeek, request.StartTime,
                request.DurationMinutes, request.Title, request.Description, request.StartDate.Value,
                request.EndDate, request.Timezone);
            if (series == null)
                return NotFound();
            var response = await ToSeriesResponse(series);
            return CreatedAtAction(nameof(GetSeries), new { seriesId = series.Id }, response);
        }
        catch (LessonValidationException exception)
        {
            return BadRequest(new { message = exception.Message });
        }
    }

    [HttpPut("{lessonId:int}")]
    public async Task<ActionResult<LessonResponseDto>> UpdateLesson(int lessonId, LessonUpdateRequestDto request)
    {
        if (!TryGetRequester(out var requester))
            return Unauthorized();
        if (!request.ScheduledAt.HasValue)
            return BadRequest(new { message = "Lesson start date-time is required." });
        try
        {
            var lesson = await _lessons.UpdateLessonAsync(
                requester.UserId, requester.Role, requester.MembershipId, lessonId,
                request.Title, request.Description, request.CourseId, request.ScheduledAt.Value,
                request.DurationMinutes, request.Timezone, request.Notes);
            return lesson == null ? NotFound() : Ok(MapLesson(lesson));
        }
        catch (LessonValidationException exception)
        {
            return BadRequest(new { message = exception.Message });
        }
        catch (LessonConflictException exception)
        {
            return Conflict(new { message = exception.Message });
        }
    }

    [HttpPut("series/{seriesId:int}")]
    public async Task<IActionResult> UpdateSeries(int seriesId, LessonSeriesUpdateRequestDto request)
    {
        if (!TryGetRequester(out var requester))
            return Unauthorized();
        try
        {
            var updated = await _lessons.UpdateSeriesAsync(
                requester.UserId, requester.Role, requester.MembershipId, seriesId,
                request.DayOfWeek, request.StartTime, request.DurationMinutes,
                request.Title, request.Description, request.Timezone);
            return updated == null ? NotFound() : NoContent();
        }
        catch (LessonValidationException exception)
        {
            return BadRequest(new { message = exception.Message });
        }
        catch (LessonConflictException exception)
        {
            return Conflict(new { message = exception.Message });
        }
    }

    [HttpPut("{lessonId:int}/status")]
    public async Task<IActionResult> ChangeStatus(int lessonId, LessonStatusRequestDto request)
    {
        if (!TryGetRequester(out var requester))
            return Unauthorized();
        if (!Enum.TryParse<LessonStatus>(request.Status, true, out var status) || !Enum.IsDefined(status))
            return BadRequest(new { message = "Unknown lesson status." });
        try
        {
            var updated = await _lessons.ChangeStatusAsync(
                requester.UserId, requester.Role, requester.MembershipId, lessonId, status, request.Reason);
            return updated == null ? NotFound() : NoContent();
        }
        catch (LessonValidationException exception)
        {
            return BadRequest(new { message = exception.Message });
        }
        catch (LessonConflictException exception)
        {
            return Conflict(new { message = exception.Message });
        }
    }

    [HttpDelete("{lessonId:int}")]
    public async Task<IActionResult> CancelLesson(int lessonId)
    {
        if (!TryGetRequester(out var requester))
            return Unauthorized();
        try
        {
            var updated = await _lessons.ChangeStatusAsync(
                requester.UserId, requester.Role, requester.MembershipId, lessonId, LessonStatus.Cancelled);
            return updated == null ? NotFound() : NoContent();
        }
        catch (LessonValidationException exception)
        {
            return BadRequest(new { message = exception.Message });
        }
        catch (LessonConflictException exception)
        {
            return Conflict(new { message = exception.Message });
        }
    }

    [HttpPost("{lessonId:int}/reschedule")]
    public async Task<ActionResult<LessonResponseDto>> Reschedule(
        int lessonId, LessonRescheduleRequestDto request)
    {
        if (!TryGetRequester(out var requester))
            return Unauthorized();
        if (!request.ScheduledAt.HasValue)
            return BadRequest(new { message = "New lesson start date-time is required." });
        try
        {
            var lesson = await _lessons.RescheduleAsync(
                requester.UserId, requester.Role, requester.MembershipId,
                lessonId, request.ScheduledAt.Value, request.Reason);
            return lesson == null ? NotFound() : Ok(MapLesson(lesson));
        }
        catch (LessonConflictException exception)
        {
            return Conflict(new { message = exception.Message });
        }
    }

    private async Task<LessonSeriesResponseDto> ToSeriesResponse(LessonSeriesEntity series)
    {
        return new LessonSeriesResponseDto
        {
            Id = series.Id,
            StudentId = series.StudentId,
            TeacherId = series.TeacherId,
            CourseId = series.CourseId,
            DayOfWeek = series.DayOfWeek,
            StartTime = series.StartTime,
            DurationMinutes = (int)series.Duration.TotalMinutes,
            Title = series.Title,
            StartDate = series.StartDate,
            EndDate = series.EndDate,
            Timezone = series.Timezone,
            GeneratedInstances = await _lessons.CountSeriesInstancesAsync(series.Id)
        };
    }

    private static LessonResponseDto MapLesson(LessonEntity lesson)
    {
        var zone = TimeZoneInfo.FindSystemTimeZoneById(lesson.Timezone);
        var utc = new DateTimeOffset(DateTime.SpecifyKind(lesson.ScheduledAtUtc, DateTimeKind.Utc));
        var local = TimeZoneInfo.ConvertTime(utc, zone);
        var history = lesson.RescheduleHistoryAsOriginal.Concat(lesson.RescheduleHistoryAsNew)
            .OrderBy(item => item.RescheduleDateUtc)
            .Select(item => new LessonRescheduleResponseDto
            {
                OriginalLessonId = item.OriginalLessonId,
                NewLessonId = item.NewLessonId,
                Reason = item.RescheduleReason,
                RescheduledAt = new DateTimeOffset(DateTime.SpecifyKind(item.RescheduleDateUtc, DateTimeKind.Utc)),
                RescheduledByUserId = item.RescheduleByUserId
            }).ToList();

        return new LessonResponseDto
        {
            Id = lesson.Id,
            SeriesId = lesson.SeriesId,
            TeacherId = lesson.TeacherId,
            StudentId = lesson.StudentId,
            StudentName = lesson.Student == null ? "" : $"{lesson.Student.FirstName} {lesson.Student.LastName}",
            CourseId = lesson.CourseId,
            CourseName = lesson.Course?.Name,
            ScheduledAt = local,
            ScheduledAtUtc = utc,
            Title = lesson.Title,
            Description = lesson.Description,
            Status = lesson.Status.ToString(),
            DurationMinutes = (int)lesson.Duration.TotalMinutes,
            Timezone = lesson.Timezone,
            Notes = lesson.Notes,
            StatusReason = lesson.StatusReason,
            ModifiedAtUtc = new DateTimeOffset(DateTime.SpecifyKind(lesson.ModifiedAtUtc, DateTimeKind.Utc)),
            ModifiedByUserId = lesson.ModifiedByUserId,
            RescheduledFromId = lesson.RescheduledFromId,
            SeriesInstanceIndex = lesson.SeriesInstanceIndex,
            RescheduleHistory = history
        };
    }
}
