namespace backend.Core.Entities;

public class StudentEntity
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public int? SchoolId { get; set; }
    public int? StandaloneTeacherId { get; set; }
    public string FirstName { get; set; } = null!;
    public string LastName { get; set; } = null!;
    public string? PhoneNumber { get; set; }
    public AccountStatus Status { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }

    // Связи
    public UserEntity User { get; set; } = null!;
    public SchoolEntity? School { get; set; }
    public TeacherEntity? StandaloneTeacher { get; set; }
    public ICollection<StudentTeacherAssignmentEntity> TeacherAssignments { get; set; } = new List<StudentTeacherAssignmentEntity>();
}
