namespace backend.Infrastructure.Services;

using backend.Core.Entities;
using backend.Core.Interfaces.Repositories;
using backend.Core.Interfaces.Services;

public class WorkspaceService : IWorkspaceService
{
    private readonly IWorkspaceMembershipRepository _membershipRepository;
    private readonly IStudentRepository _studentRepository;
    private readonly ITeacherRepository _teacherRepository;

    public WorkspaceService(
        IWorkspaceMembershipRepository membershipRepository,
        IStudentRepository studentRepository,
        ITeacherRepository teacherRepository)
    {
        _membershipRepository = membershipRepository;
        _studentRepository = studentRepository;
        _teacherRepository = teacherRepository;
    }

    public async Task<IEnumerable<WorkspaceMembershipEntity>> GetUserWorkspacesAsync(int userId)
    {
        return await _membershipRepository.GetByUserIdAsync(userId);
    }

    public async Task<WorkspaceMembershipEntity?> GetWorkspaceMembershipAsync(int userId, int membershipId)
    {
        var membership = await _membershipRepository.GetByIdAsync(membershipId);
        if (membership?.UserId != userId)
            return null;

        return membership;
    }

    public async Task<bool> CanAccessWorkspaceAsync(int userId, int membershipId)
    {
        var membership = await _membershipRepository.GetByIdAsync(membershipId);
        return membership?.UserId == userId;
    }

    public async Task<bool> CanAccessSchoolAsync(int userId, int schoolId)
    {
        var membership = await _membershipRepository.GetUserSchoolMembershipAsync(userId, schoolId);
        return membership != null;
    }

    public async Task<bool> CanViewStudentAsync(int userId, int studentId, int membershipId)
    {
        var membership = await GetWorkspaceMembershipAsync(userId, membershipId);
        if (membership == null)
            return false;

        var student = await _studentRepository.GetByIdAsync(studentId);
        if (student == null)
            return false;

        if (membership.WorkspaceType == WorkspaceType.School && membership.SchoolId == student.SchoolId)
            return true;

        if (membership.WorkspaceType == WorkspaceType.Standalone)
        {
            var teacher = await _teacherRepository.GetByUserIdAsync(userId);
            if (teacher != null && student.StandaloneTeacherId == teacher.Id)
                return true;
        }

        return false;
    }
}
