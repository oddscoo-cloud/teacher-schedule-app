namespace backend.Core.Entities;

public enum StudentLessonChargeType
{
    Completed,
    LateCancellation
}

public class StudentLessonChargeEntity
{
    public int Id { get; set; }
    public int StudentId { get; set; }
    public int SourceLessonId { get; set; }
    public StudentLessonChargeType ChargeType { get; set; }
    public DateTimeOffset LessonStartsAt { get; set; }
    public DateTimeOffset ChargedAt { get; set; }
    public decimal Amount { get; set; }
    public int RecordedByUserId { get; set; }
    public StudentEntity Student { get; set; } = null!;
    public UserEntity RecordedByUser { get; set; } = null!;
}
