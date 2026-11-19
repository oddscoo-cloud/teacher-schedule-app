namespace backend.Core.Entities;

public class CourseEntity
{
    public int Id { get; set; }
    public int SchoolId { get; set; }
    public string Name { get; set; } = null!;
    public string? Description { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public SchoolEntity School { get; set; } = null!;
    public ICollection<StudentCourseAssignmentEntity> Students { get; set; } = new List<StudentCourseAssignmentEntity>();
}
