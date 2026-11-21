namespace backend.Core.Entities;

public class LessonEntity
{
    public int Id { get; set; }
    public int? SeriesId { get; set; }
    public int TeacherId { get; set; }
    public int StudentId { get; set; }
    public int? SchoolId { get; set; }
    public int? CourseId { get; set; }
    public DateTime ScheduledAtUtc { get; set; }
    public string Title { get; set; } = null!;
    public string? Description { get; set; }
    public LessonStatus Status { get; set; } = LessonStatus.Scheduled;
    public TimeSpan Duration { get; set; }
    public string Timezone { get; set; } = "UTC";
    public string? Notes { get; set; }
    public string? StatusReason { get; set; }
    public DateTime CreatedAtUtc { get; set; }
    public DateTime ModifiedAtUtc { get; set; }
    public int? ModifiedByUserId { get; set; }
    public int? RescheduledFromId { get; set; }
    public int? SeriesInstanceIndex { get; set; }

    public LessonSeriesEntity? Series { get; set; }
    public StudentEntity Student { get; set; } = null!;
    public CourseEntity? Course { get; set; }
    public LessonEntity? RescheduledFrom { get; set; }
    public ICollection<LessonEntity> RescheduledTo { get; set; } = new List<LessonEntity>();
    public ICollection<LessonRescheduleEntity> RescheduleHistoryAsOriginal { get; set; } = new List<LessonRescheduleEntity>();
    public ICollection<LessonRescheduleEntity> RescheduleHistoryAsNew { get; set; } = new List<LessonRescheduleEntity>();
}
