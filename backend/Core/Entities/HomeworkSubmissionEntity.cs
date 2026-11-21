namespace backend.Core.Entities;

public class HomeworkSubmissionEntity
{
    public int Id { get; set; }
    public int HomeworkId { get; set; }
    public int AttemptNumber { get; set; }
    public string? Text { get; set; }
    public string LinksJson { get; set; } = "[]";
    public int? Grade { get; set; }
    public DateTime SubmittedAt { get; set; }
    public DateTime? GradedAt { get; set; }

    public HomeworkEntity Homework { get; set; } = null!;
    public ICollection<HomeworkAttachmentEntity> Attachments { get; set; } = new List<HomeworkAttachmentEntity>();
}
