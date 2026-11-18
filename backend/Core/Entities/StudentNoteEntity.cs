namespace backend.Core.Entities;

public class StudentNoteEntity
{
    public int Id { get; set; }
    public int StudentId { get; set; }
    public int TeacherId { get; set; }
    public string Content { get; set; } = null!;
    public string? Goals { get; set; }
    public string? Progress { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public StudentEntity Student { get; set; } = null!;
    public TeacherEntity Teacher { get; set; } = null!;
}
