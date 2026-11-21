namespace backend.Core.Interfaces.Services;

using backend.Core.Entities;

public interface IHomeworkService
{
    Task<IReadOnlyList<HomeworkEntity>?> GetHomeworksAsync(
        int userId,
        UserRole role,
        int? membershipId,
        int? studentId,
        int? courseId,
        bool pendingOnly);

    Task<HomeworkEntity?> GetHomeworkAsync(int userId, UserRole role, int? membershipId, int homeworkId);

    Task<HomeworkEntity?> CreateHomeworkAsync(int userId, UserRole role, int membershipId, HomeworkEntity homework);

    Task<HomeworkSubmissionEntity?> SubmitAsync(
        int userId,
        UserRole role,
        int homeworkId,
        string? text,
        IReadOnlyCollection<string> links,
        IReadOnlyCollection<Microsoft.AspNetCore.Http.IFormFile> files);

    Task<bool?> GradeAsync(int userId, UserRole role, int? membershipId, int submissionId, int grade);

    Task<HomeworkAttachmentEntity?> GetAttachmentAsync(
        int userId,
        UserRole role,
        int? membershipId,
        int attachmentId);
}
