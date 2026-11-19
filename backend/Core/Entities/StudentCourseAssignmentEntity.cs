namespace backend.Core.Entities;

public class StudentCourseAssignmentEntity
{
    public int StudentId { get; set; }
    public int CourseId { get; set; }
    public StudentEntity Student { get; set; } = null!;
    public CourseEntity Course { get; set; } = null!;
}
