namespace backend.Core.Entities;

public class StudentPriceHistoryEntity
{
    public int Id { get; set; }
    public int StudentId { get; set; }
    public decimal Amount { get; set; }
    public DateTimeOffset EffectiveFrom { get; set; }
    public DateTimeOffset RecordedAt { get; set; }
    public int RecordedByUserId { get; set; }
    public StudentEntity Student { get; set; } = null!;
    public UserEntity RecordedByUser { get; set; } = null!;
}
