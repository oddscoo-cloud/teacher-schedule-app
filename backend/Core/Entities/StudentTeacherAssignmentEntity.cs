namespace backend.Core.Entities;

public class StudentTeacherAssignmentEntity
{
    public int Id { get; set; }
    public int StudentId { get; set; }
    public int TeacherId { get; set; }
    public DateTime AssignedAt { get; set; }

    // Связи
    public StudentEntity Student { get; set; } = null!;
    public TeacherEntity Teacher { get; set; } = null!;
}
