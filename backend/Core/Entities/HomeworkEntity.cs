namespace backend.Core.Entities;

public class HomeworkEntity
{
    public int Id { get; set; }
    public int StudentId { get; set; }
    public int TeacherId { get; set; }
    public int? SchoolId { get; set; }
    public int? CourseId { get; set; }
    public string Title { get; set; } = null!;
    public string? Instructions { get; set; }
    public DateTime? DueAt { get; set; }
    public DateTime CreatedAt { get; set; }

    public StudentEntity Student { get; set; } = null!;
    public TeacherEntity Teacher { get; set; } = null!;
    public SchoolEntity? School { get; set; }
    public CourseEntity? Course { get; set; }
    public ICollection<HomeworkSubmissionEntity> Submissions { get; set; } = new List<HomeworkSubmissionEntity>();
}
