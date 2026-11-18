namespace backend.Core.Interfaces.Services;

using backend.Core.Entities;

public interface IStudentService
{
    Task<IReadOnlyList<StudentEntity>?> GetStudentsAsync(
        int userId,
        UserRole role,
        int membershipId,
        string? search,
        int? courseId,
        AccountStatus? status);

    Task<StudentEntity?> GetStudentAsync(int userId, UserRole role, int membershipId, int studentId);
    Task<StudentEntity?> CreateStudentAsync(
        int userId,
        UserRole role,
        int membershipId,
        StudentEntity student,
        IReadOnlyCollection<int> courseIds,
        IReadOnlyCollection<StudentContactEntity> contacts);

    Task<StudentEntity?> UpdateStudentAsync(
        int userId,
        UserRole role,
        int membershipId,
        int studentId,
        StudentEntity updatedStudent,
        IReadOnlyCollection<int> courseIds,
        IReadOnlyCollection<StudentContactEntity> contacts);
}
