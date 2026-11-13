namespace backend.Core.Use_Cases.Auth;

using backend.Core.Entities;
using backend.Core.Interfaces.Repositories;
using backend.Core.Interfaces.Services;

public class CreateSchoolAdminUseCase
{
    private readonly IUserRepository _userRepository;
    private readonly ISchoolRepository _schoolRepository;
    private readonly IWorkspaceMembershipRepository _membershipRepository;
    private readonly IAuthService _authService;

    public CreateSchoolAdminUseCase(
        IUserRepository userRepository,
        ISchoolRepository schoolRepository,
        IWorkspaceMembershipRepository membershipRepository,
        IAuthService authService)
    {
        _userRepository = userRepository;
        _schoolRepository = schoolRepository;
        _membershipRepository = membershipRepository;
        _authService = authService;
    }

    public async Task<(bool success, string message, int? schoolId, int? userId)> ExecuteAsync(
        string adminEmail,
        string adminPassword,
        string schoolName,
        string bootstrapSecret)
    {
        var existingUser = await _userRepository.GetByEmailAsync(adminEmail);
        if (existingUser != null)
            return (false, "Admin email already registered", null, null);

        var existingSchool = await _schoolRepository.GetDefaultSchoolAsync();
        if (existingSchool != null)
            return (false, "School already exists. Only one school per installation in MVP.", null, null);

        if (string.IsNullOrWhiteSpace(adminPassword) || adminPassword.Length < 6)
            return (false, "Password must be at least 6 characters", null, null);

        var adminUser = new UserEntity
        {
            Email = adminEmail,
            PasswordHash = _authService.HashPassword(adminPassword),
            Role = UserRole.SchoolAdmin,
            Status = AccountStatus.Active
        };

        await _userRepository.CreateAsync(adminUser);

        var school = new SchoolEntity
        {
            Name = schoolName
        };

        await _schoolRepository.CreateAsync(school);

        var membership = new WorkspaceMembershipEntity
        {
            UserId = adminUser.Id,
            SchoolId = school.Id,
            WorkspaceType = WorkspaceType.School,
            IsDefault = true
        };

        await _membershipRepository.CreateAsync(membership);

        return (true, "School admin and school created successfully", school.Id, adminUser.Id);
    }
}
