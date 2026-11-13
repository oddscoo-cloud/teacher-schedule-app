namespace backend.Core.Use_Cases.Auth;

using backend.Core.Entities;
using backend.Core.Interfaces.Repositories;
using backend.Core.Interfaces.Services;

public class InviteSchoolTeacherUseCase
{
    private readonly IUserRepository _userRepository;
    private readonly IWorkspaceMembershipRepository _membershipRepository;
    private readonly ISchoolRepository _schoolRepository;
    private readonly ITeacherRepository _teacherRepository;
    private readonly IAuthService _authService;

    public InviteSchoolTeacherUseCase(
        IUserRepository userRepository,
        IWorkspaceMembershipRepository membershipRepository,
        ISchoolRepository schoolRepository,
        ITeacherRepository teacherRepository,
        IAuthService authService)
    {
        _userRepository = userRepository;
        _membershipRepository = membershipRepository;
        _schoolRepository = schoolRepository;
        _teacherRepository = teacherRepository;
        _authService = authService;
    }

    public async Task<(bool success, string message)> ExecuteAsync(string teacherEmail, int schoolId)
    {
        var school = await _schoolRepository.GetByIdAsync(schoolId);
        if (school == null)
            return (false, "School not found");

        var existingUser = await _userRepository.GetByEmailAsync(teacherEmail);

        if (existingUser != null)
        {
            // Teacher already registered - add them to school
            if (existingUser.Role != UserRole.Teacher)
                return (false, "User is not a teacher");

            var existingMembership = await _membershipRepository.GetUserSchoolMembershipAsync(existingUser.Id, schoolId);
            if (existingMembership != null)
                return (false, "Teacher already invited to this school");

            var membership = new WorkspaceMembershipEntity
            {
                UserId = existingUser.Id,
                SchoolId = schoolId,
                WorkspaceType = WorkspaceType.School,
                IsDefault = false
            };

            await _membershipRepository.CreateAsync(membership);

            // Create teacher record if doesn't exist for this school
            var teacherRecord = await _teacherRepository.GetByUserIdAsync(existingUser.Id);
            if (teacherRecord == null || teacherRecord.SchoolId != schoolId)
            {
                var newTeacher = new TeacherEntity
                {
                    UserId = existingUser.Id,
                    SchoolId = schoolId,
                    FirstName = "Pending", // TODO: Get from profile
                    LastName = "Setup"
                };
                await _teacherRepository.CreateAsync(newTeacher);
            }

            return (true, "Teacher added to school successfully");
        }

        // New teacher - create account with pending status
        var newUser = new UserEntity
        {
            Email = teacherEmail,
            PasswordHash = _authService.HashPassword(Guid.NewGuid().ToString().Substring(0, 12)),
            Role = UserRole.Teacher,
            Status = AccountStatus.Pending
        };

        await _userRepository.CreateAsync(newUser);

        var schoolMembership = new WorkspaceMembershipEntity
        {
            UserId = newUser.Id,
            SchoolId = schoolId,
            WorkspaceType = WorkspaceType.School,
            IsDefault = true
        };

        await _membershipRepository.CreateAsync(schoolMembership);

        var teacher = new TeacherEntity
        {
            UserId = newUser.Id,
            SchoolId = schoolId,
            FirstName = "Pending",
            LastName = "Setup"
        };

        await _teacherRepository.CreateAsync(teacher);

        return (true, "Teacher invitation sent. They will receive setup instructions via email (implementation pending)");
    }
}
