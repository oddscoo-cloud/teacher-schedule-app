namespace backend.Core.Interfaces.Repositories;

using backend.Core.Entities;

public interface IWorkspaceMembershipRepository
{
    Task<WorkspaceMembershipEntity?> GetByIdAsync(int id);
    Task<IEnumerable<WorkspaceMembershipEntity>> GetByUserIdAsync(int userId);
    Task<WorkspaceMembershipEntity?> GetUserSchoolMembershipAsync(int userId, int schoolId);
    Task<WorkspaceMembershipEntity?> GetDefaultMembershipAsync(int userId);
    Task<WorkspaceMembershipEntity> CreateAsync(WorkspaceMembershipEntity membership);
    Task<WorkspaceMembershipEntity> UpdateAsync(WorkspaceMembershipEntity membership);
    Task DeleteAsync(int id);
}
