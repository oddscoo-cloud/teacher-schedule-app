namespace backend.Infrastructure.Services;

using backend.Core.Entities;
using backend.Core.Interfaces.Repositories;
using backend.Core.Interfaces.Services;

public class WorkspaceService : IWorkspaceService
{
    private readonly IWorkspaceMembershipRepository _membershipRepository;
    private readonly IStudentRepository _studentRepository;
    private readonly ITeacherRepository _teacherRepository;
    private readonly IUserRepository _userRepository;

    public WorkspaceService(
        IWorkspaceMembershipRepository membershipRepository,
        IStudentRepository studentRepository,
        ITeacherRepository teacherRepository,
        IUserRepository userRepository)
    {
        _membershipRepository = membershipRepository;
        _studentRepository = studentRepository;
        _teacherRepository = teacherRepository;
        _userRepository = userRepository;
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

        var user = await _userRepository.GetByIdAsync(userId);
        if (user == null)
            return false;

        if (membership.WorkspaceType == WorkspaceType.School &&
            membership.SchoolId.HasValue &&
            membership.SchoolId == student.SchoolId)
        {
            if (user.Role == UserRole.SchoolAdmin)
                return true;
            if (user.Role != UserRole.Teacher)
                return false;

            var schoolTeacher = await _teacherRepository.GetByUserIdAndSchoolIdAsync(userId, membership.SchoolId);
            return schoolTeacher != null && student.TeacherAssignments.Any(assignment =>
                assignment.TeacherId == schoolTeacher.Id && assignment.IsSchool);
        }

        if (membership.WorkspaceType == WorkspaceType.Standalone && user.Role == UserRole.Teacher)
        {
            var teacher = await _teacherRepository.GetByUserIdAndSchoolIdAsync(userId, null);
            if (teacher != null &&
                student.SchoolId == null &&
                student.StandaloneTeacherId == teacher.Id)
                return true;
        }

        return false;
    }
}
