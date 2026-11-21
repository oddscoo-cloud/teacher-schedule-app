namespace backend.Core.Entities;

public class StudentEntity
{
    public int Id { get; set; }
    public int? UserId { get; set; }
    public int? SchoolId { get; set; }
    public int? StandaloneTeacherId { get; set; }
    public string FirstName { get; set; } = null!;
    public string LastName { get; set; } = null!;
    public string? Patronymic { get; set; }
    public string? Email { get; set; }
    public string? PhoneNumber { get; set; }
    public DateTime? DateOfBirth { get; set; }
    public AccountStatus Status { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }

    // Связи
    public UserEntity? User { get; set; }
    public SchoolEntity? School { get; set; }
    public TeacherEntity? StandaloneTeacher { get; set; }
    public ICollection<StudentAssignmentEntity> TeacherAssignments { get; set; } = new List<StudentAssignmentEntity>();
    public ICollection<StudentContactEntity> Contacts { get; set; } = new List<StudentContactEntity>();
    public ICollection<StudentCourseAssignmentEntity> CourseAssignments { get; set; } = new List<StudentCourseAssignmentEntity>();
    public ICollection<StudentNoteEntity> Notes { get; set; } = new List<StudentNoteEntity>();
    public ICollection<StudentPriceHistoryEntity> PriceHistory { get; set; } = new List<StudentPriceHistoryEntity>();
    public ICollection<StudentPaymentEntity> Payments { get; set; } = new List<StudentPaymentEntity>();
    public ICollection<StudentLessonChargeEntity> LessonCharges { get; set; } = new List<StudentLessonChargeEntity>();
}
