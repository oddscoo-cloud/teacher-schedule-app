namespace backend.Core.Entities;

public class HomeworkAttachmentEntity
{
    public int Id { get; set; }
    public int SubmissionId { get; set; }
    public string StorageName { get; set; } = null!;
    public string DisplayName { get; set; } = null!;
    public string ContentType { get; set; } = null!;
    public long SizeBytes { get; set; }

    public HomeworkSubmissionEntity Submission { get; set; } = null!;
}
