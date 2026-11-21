namespace backend.Infrastructure.Services;

public sealed class HomeworkFileStorageOptions
{
    public string StoragePath { get; set; } = string.Empty;
    public long MaxFileSizeBytes { get; set; } = 5 * 1024 * 1024;
    public int MaxFilesPerSubmission { get; set; } = 5;
    public string[] AllowedContentTypes { get; set; } = ["image/jpeg", "image/png"];
}
