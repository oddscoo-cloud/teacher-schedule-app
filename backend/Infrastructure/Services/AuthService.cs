namespace backend.Infrastructure.Services;

using backend.Core.Entities;
using backend.Core.Interfaces.Repositories;
using backend.Core.Interfaces.Services;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

public class AuthService : IAuthService
{
    private readonly IUserRepository _userRepository;
    private readonly IWorkspaceMembershipRepository _workspaceMembershipRepository;
    private readonly ITeacherRepository _teacherRepository;
    private readonly IStudentRepository _studentRepository;
    private readonly ISchoolRepository _schoolRepository;
    private readonly IConfiguration _configuration;

    public AuthService(
        IUserRepository userRepository,
        IWorkspaceMembershipRepository workspaceMembershipRepository,
        ITeacherRepository teacherRepository,
        IStudentRepository studentRepository,
        ISchoolRepository schoolRepository,
        IConfiguration configuration)
    {
        _userRepository = userRepository;
        _workspaceMembershipRepository = workspaceMembershipRepository;
        _teacherRepository = teacherRepository;
        _studentRepository = studentRepository;
        _schoolRepository = schoolRepository;
        _configuration = configuration;
    }

    public async Task<LoginResult> LoginAsync(string email, string password)
    {
        var user = await _userRepository.GetByEmailAsync(email);

        if (user == null)
            return new LoginResult { Success = false, Message = "Invalid email or password" };

        if (user.Status != AccountStatus.Active)
            return new LoginResult { Success = false, Message = "Account is not active" };

        if (!VerifyPassword(password, user.PasswordHash))
            return new LoginResult { Success = false, Message = "Invalid email or password" };

        return new LoginResult { Success = true, User = user };
    }

    public async Task<(bool success, string message)> RegisterStudentAsync(string email, string password, string firstName, string lastName)
    {
        var existingUser = await _userRepository.GetByEmailAsync(email);
        if (existingUser != null)
            return (false, "Email already registered");

        if (string.IsNullOrWhiteSpace(password) || password.Length < 6)
            return (false, "Password must be at least 6 characters");

        var user = new UserEntity
        {
            Email = email,
            PasswordHash = HashPassword(password),
            Role = UserRole.Student,
            Status = AccountStatus.Pending
        };

        await _userRepository.CreateAsync(user);

        var student = new StudentEntity
        {
            UserId = user.Id,
            FirstName = firstName,
            LastName = lastName,
            Email = email,
            Status = AccountStatus.Pending
        };

        await _studentRepository.CreateAsync(student);

        return (true, "Registration successful. Awaiting confirmation from teacher or school administrator.");
    }

    public async Task<(bool success, string message)> RegisterStandaloneTeacherAsync(string email, string password, string firstName, string lastName)
    {
        var existingUser = await _userRepository.GetByEmailAsync(email);
        if (existingUser != null)
            return (false, "Email already registered");

        if (string.IsNullOrWhiteSpace(password) || password.Length < 6)
            return (false, "Password must be at least 6 characters");

        var user = new UserEntity
        {
            Email = email,
            PasswordHash = HashPassword(password),
            Role = UserRole.Teacher,
            Status = AccountStatus.Active
        };

        await _userRepository.CreateAsync(user);

        var teacher = new TeacherEntity
        {
            UserId = user.Id,
            FirstName = firstName,
            LastName = lastName,
            SchoolId = null
        };

        await _teacherRepository.CreateAsync(teacher);

        // Создаём standalone workspace
        var membership = new WorkspaceMembershipEntity
        {
            UserId = user.Id,
            SchoolId = null,
            WorkspaceType = WorkspaceType.Standalone,
            IsDefault = true
        };

        await _workspaceMembershipRepository.CreateAsync(membership);

        return (true, "Registration successful. You can now log in.");
    }

    public async Task<(bool success, string message)> InviteSchoolTeacherAsync(string email, int schoolId)
    {
        var school = await _schoolRepository.GetByIdAsync(schoolId);
        if (school == null)
            return (false, "School not found");

        var existingUser = await _userRepository.GetByEmailAsync(email);
        if (existingUser != null)
        {
            // Если пользователь уже существует, добавляем его в школу
            var existingMembership = await _workspaceMembershipRepository.GetUserSchoolMembershipAsync(existingUser.Id, schoolId);
            if (existingMembership != null)
                return (false, "Teacher already invited to this school");

            var membership = new WorkspaceMembershipEntity
            {
                UserId = existingUser.Id,
                SchoolId = schoolId,
                WorkspaceType = WorkspaceType.School,
                IsDefault = false
            };

            await _workspaceMembershipRepository.CreateAsync(membership);
            return (true, "Teacher invited to school");
        }

        // Создаём нового учителя с pending статусом
        var newUser = new UserEntity
        {
            Email = email,
            PasswordHash = HashPassword(GenerateTemporaryPassword()),
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

        await _workspaceMembershipRepository.CreateAsync(schoolMembership);

        return (true, "Teacher invitation sent. They will receive credentials via email (implementation pending).");
    }

    public async Task<(bool success, string message)> ConfirmStudentAsync(int studentId, int? teacherId, int? schoolId)
    {
        var student = await _studentRepository.GetByIdAsync(studentId);
        if (student == null)
            return (false, "Student not found");

        if (schoolId.HasValue)
        {
            var school = await _schoolRepository.GetByIdAsync(schoolId.Value);
            if (school == null)
                return (false, "School not found");

            student.SchoolId = schoolId.Value;
            student.Status = AccountStatus.Active;
        }

        if (teacherId.HasValue)
        {
            var teacher = await _teacherRepository.GetByIdAsync(teacherId.Value);
            if (teacher == null)
                return (false, "Teacher not found");

            var assignment = new StudentAssignmentEntity
            {
                StudentId = studentId,
                TeacherId = teacherId.Value,
                IsSchool = schoolId.HasValue
            };

            student.TeacherAssignments.Add(assignment);
        }

        student.Status = AccountStatus.Active;
        await _studentRepository.UpdateAsync(student);

        return (true, "Student confirmed");
    }

    public async Task<string> GenerateJwtTokenAsync(UserEntity user, int? workspaceMembershipId = null)
    {
        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new Claim(ClaimTypes.Email, user.Email),
            new Claim(ClaimTypes.Role, user.Role.ToString())
        };

        if (workspaceMembershipId.HasValue)
            claims.Add(new Claim("workspaceMembershipId", workspaceMembershipId.Value.ToString()));

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(
            _configuration["Jwt:SecretKey"] ?? throw new InvalidOperationException("JWT secret key not configured")));

        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer: _configuration["Jwt:Issuer"],
            audience: _configuration["Jwt:Audience"],
            claims: claims,
            expires: DateTime.UtcNow.AddHours(24),
            signingCredentials: creds
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    public string HashPassword(string password)
    {
        return BCrypt.Net.BCrypt.HashPassword(password);
    }

    public bool VerifyPassword(string password, string hash)
    {
        try
        {
            return BCrypt.Net.BCrypt.Verify(password, hash);
        }
        catch
        {
            return false;
        }
    }

    private string GenerateTemporaryPassword()
    {
        return Guid.NewGuid().ToString().Substring(0, 12);
    }
}
