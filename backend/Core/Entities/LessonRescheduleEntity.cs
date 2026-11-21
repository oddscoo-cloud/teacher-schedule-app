namespace backend.Core.Entities;

public class LessonRescheduleEntity
{
    public int Id { get; set; }
    public int OriginalLessonId { get; set; }
    public int NewLessonId { get; set; }
    public string? RescheduleReason { get; set; }
    public DateTime RescheduleDateUtc { get; set; }
    public int RescheduleByUserId { get; set; }

    public LessonEntity OriginalLesson { get; set; } = null!;
    public LessonEntity NewLesson { get; set; } = null!;
}
