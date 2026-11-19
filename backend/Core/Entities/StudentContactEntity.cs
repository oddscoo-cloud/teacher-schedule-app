namespace backend.Core.Entities;

public class StudentContactEntity
{
    public int StudentId { get; set; }
    public int ContactId { get; set; }
    public string? Relationship { get; set; }
    public StudentEntity Student { get; set; } = null!;
    public ContactEntity Contact { get; set; } = null!;
}
