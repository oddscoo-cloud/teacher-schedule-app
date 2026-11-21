namespace backend.Presentation.DTOs;

using backend.Core.Entities;
using System.ComponentModel.DataAnnotations;

public sealed class LessonRequestDto
{
    [Range(1, int.MaxValue)]
    public int StudentId { get; set; }
    public int? TeacherId { get; set; }
    public int? CourseId { get; set; }
    [Required, StringLength(255, MinimumLength = 1)]
    public string Title { get; set; } = "";
    [StringLength(4000)]
    public string? Description { get; set; }
    [Required]
    public DateTimeOffset? ScheduledAt { get; set; }
    [Range(1, 1440)]
    public int DurationMinutes { get; set; } = 60;
    [Required, StringLength(100)]
    public string Timezone { get; set; } = "";
}

public sealed class LessonSeriesRequestDto
{
    [Range(1, int.MaxValue)]
    public int StudentId { get; set; }
    public int? TeacherId { get; set; }
    public int? CourseId { get; set; }
    public DayOfWeek DayOfWeek { get; set; }
    public TimeSpan StartTime { get; set; }
    [Range(1, 1440)]
    public int DurationMinutes { get; set; } = 60;
    [Required, StringLength(255, MinimumLength = 1)]
    public string Title { get; set; } = "";
    [StringLength(4000)]
    public string? Description { get; set; }
    [Required]
    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    [Required, StringLength(100)]
    public string Timezone { get; set; } = "";
}

public sealed class LessonUpdateRequestDto
{
    public int? CourseId { get; set; }
    [Required, StringLength(255, MinimumLength = 1)]
    public string Title { get; set; } = "";
    [StringLength(4000)]
    public string? Description { get; set; }
    [Required]
    public DateTimeOffset? ScheduledAt { get; set; }
    [Range(1, 1440)]
    public int DurationMinutes { get; set; }
    [Required, StringLength(100)]
    public string Timezone { get; set; } = "";
    [StringLength(4000)]
    public string? Notes { get; set; }
}

public sealed class LessonSeriesUpdateRequestDto
{
    public DayOfWeek DayOfWeek { get; set; }
    public TimeSpan StartTime { get; set; }
    [Range(1, 1440)]
    public int DurationMinutes { get; set; }
    [Required, StringLength(255, MinimumLength = 1)]
    public string Title { get; set; } = "";
    [StringLength(4000)]
    public string? Description { get; set; }
    [Required, StringLength(100)]
    public string Timezone { get; set; } = "";
}

public sealed class LessonStatusRequestDto
{
    [Required]
    public string Status { get; set; } = "";
    [StringLength(500)]
    public string? Reason { get; set; }
}

public sealed class LessonSeriesResponseDto
{
    public int Id { get; set; }
    public int StudentId { get; set; }
    public int TeacherId { get; set; }
    public int? CourseId { get; set; }
    public DayOfWeek DayOfWeek { get; set; }
    public TimeSpan StartTime { get; set; }
    public int DurationMinutes { get; set; }
    public string Title { get; set; } = "";
    public DateTime StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public string Timezone { get; set; } = "";
    public int GeneratedInstances { get; set; }
}

public sealed class LessonRescheduleRequestDto
{
    [Required]
    public DateTimeOffset? ScheduledAt { get; set; }
    [StringLength(500)]
    public string? Reason { get; set; }
}

public sealed class LessonResponseDto
{
    public int Id { get; set; }
    public int? SeriesId { get; set; }
    public int TeacherId { get; set; }
    public int StudentId { get; set; }
    public string StudentName { get; set; } = "";
    public int? CourseId { get; set; }
    public string? CourseName { get; set; }
    public DateTimeOffset ScheduledAt { get; set; }
    public DateTimeOffset ScheduledAtUtc { get; set; }
    public string Title { get; set; } = "";
    public string? Description { get; set; }
    public string Status { get; set; } = "";
    public int DurationMinutes { get; set; }
    public string Timezone { get; set; } = "";
    public string? Notes { get; set; }
    public string? StatusReason { get; set; }
    public DateTimeOffset ModifiedAtUtc { get; set; }
    public int? ModifiedByUserId { get; set; }
    public int? RescheduledFromId { get; set; }
    public int? SeriesInstanceIndex { get; set; }
    public IReadOnlyList<LessonRescheduleResponseDto> RescheduleHistory { get; set; } = Array.Empty<LessonRescheduleResponseDto>();
}

public sealed class LessonRescheduleResponseDto
{
    public int OriginalLessonId { get; set; }
    public int NewLessonId { get; set; }
    public string? Reason { get; set; }
    public DateTimeOffset RescheduledAt { get; set; }
    public int RescheduledByUserId { get; set; }
}
