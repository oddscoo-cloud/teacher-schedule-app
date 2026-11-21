namespace backend.Core.Entities;

public class StudentPaymentAdjustmentEntity
{
    public int Id { get; set; }
    public int PaymentId { get; set; }
    public decimal AmountDelta { get; set; }
    public int PaidLessonCountDelta { get; set; }
    public string Reason { get; set; } = null!;
    public DateTimeOffset RecordedAt { get; set; }
    public int RecordedByUserId { get; set; }
    public string IdempotencyKey { get; set; } = null!;
    public StudentPaymentEntity Payment { get; set; } = null!;
    public UserEntity RecordedByUser { get; set; } = null!;
}
