namespace backend.Core.Entities;

public class TeacherEntity
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public int? SchoolId { get; set; }
    public string FirstName { get; set; } = null!;
    public string LastName { get; set; } = null!;
    public string? Bio { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }

    // Связи
    public UserEntity User { get; set; } = null!;
    public SchoolEntity? School { get; set; }
    public ICollection<StudentTeacherAssignmentEntity> StudentAssignments { get; set; } = new List<StudentTeacherAssignmentEntity>();
    public ICollection<StudentEntity> StandaloneStudents { get; set; } = new List<StudentEntity>();
}
