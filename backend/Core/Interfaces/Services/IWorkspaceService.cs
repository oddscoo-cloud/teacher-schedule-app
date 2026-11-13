namespace backend.Core.Interfaces.Services;

using backend.Core.Entities;

public interface IWorkspaceService
{
    Task<IEnumerable<WorkspaceMembershipEntity>> GetUserWorkspacesAsync(int userId);
    Task<WorkspaceMembershipEntity?> GetWorkspaceMembershipAsync(int userId, int membershipId);
    Task<bool> CanAccessWorkspaceAsync(int userId, int membershipId);
    Task<bool> CanAccessSchoolAsync(int userId, int schoolId);
    Task<bool> CanViewStudentAsync(int userId, int studentId, int membershipId);
}
