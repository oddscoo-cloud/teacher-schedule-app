namespace backend.Core.Interfaces.Services;

using backend.Core.Entities;

public interface IStudentAssignmentService
{
    Task<bool> AssignTeacherAsync(int userId, UserRole role, int membershipId, int studentId, int teacherId);
    Task<bool> RevokeTeacherAsync(int userId, UserRole role, int membershipId, int studentId, int teacherId);
    Task<IReadOnlyList<StudentAssignmentEntity>?> GetAssignmentsAsync(int userId, UserRole role, int membershipId, int studentId);
}
