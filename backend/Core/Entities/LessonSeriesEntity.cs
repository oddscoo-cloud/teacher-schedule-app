namespace backend.Core.Entities;

public class LessonSeriesEntity
{
    public int Id { get; set; }
    public int TeacherId { get; set; }
    public int StudentId { get; set; }
    public int? SchoolId { get; set; }
    public int? CourseId { get; set; }
    public DayOfWeek DayOfWeek { get; set; }
    public TimeSpan StartTime { get; set; }
    public TimeSpan Duration { get; set; }
    public string Title { get; set; } = null!;
    public string? Description { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public string Timezone { get; set; } = "UTC";
    public DateTime CreatedAt { get; set; }
    public bool IsActive { get; set; } = true;

    public ICollection<LessonEntity> Lessons { get; set; } = new List<LessonEntity>();
}
