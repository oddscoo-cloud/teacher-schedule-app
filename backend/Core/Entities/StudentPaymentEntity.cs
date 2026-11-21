namespace backend.Core.Entities;

public class StudentPaymentEntity
{
    public int Id { get; set; }
    public int StudentId { get; set; }
    public decimal Amount { get; set; }
    public int PaidLessonCount { get; set; }
    public DateTimeOffset PaidAt { get; set; }
    public DateTimeOffset RecordedAt { get; set; }
    public int RecordedByUserId { get; set; }
    public string IdempotencyKey { get; set; } = null!;
    public StudentEntity Student { get; set; } = null!;
    public UserEntity RecordedByUser { get; set; } = null!;
    public ICollection<StudentPaymentAdjustmentEntity> Adjustments { get; set; } = new List<StudentPaymentAdjustmentEntity>();
}
