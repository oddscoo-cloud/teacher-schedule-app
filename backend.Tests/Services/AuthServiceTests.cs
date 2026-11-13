using backend.Core.Entities;
using backend.Core.Interfaces.Repositories;
using backend.Core.Interfaces.Services;
using backend.Infrastructure.Services;
using Microsoft.Extensions.Configuration;

namespace backend.Tests.Services;

public class AuthServiceTests
{
    private readonly Mock<IUserRepository> _userRepositoryMock;
    private readonly Mock<IWorkspaceMembershipRepository> _workspaceMembershipRepositoryMock;
    private readonly Mock<ISchoolRepository> _schoolRepositoryMock;
    private readonly Mock<ITeacherRepository> _teacherRepositoryMock;
    private readonly Mock<IStudentRepository> _studentRepositoryMock;
    private readonly IAuthService _authService;

    public AuthServiceTests()
    {
        _userRepositoryMock = new Mock<IUserRepository>();
        _workspaceMembershipRepositoryMock = new Mock<IWorkspaceMembershipRepository>();
        _schoolRepositoryMock = new Mock<ISchoolRepository>();
        _teacherRepositoryMock = new Mock<ITeacherRepository>();
        _studentRepositoryMock = new Mock<IStudentRepository>();

        var jwtConfig = new Dictionary<string, string?>
        {
            { "Jwt:SecretKey", "very-secret-key-that-is-long-enough-to-be-valid-for-hmac256-encryption" },
            { "Jwt:Issuer", "TeacherScheduleApp" },
            { "Jwt:Audience", "TeacherScheduleAppUsers" }
        };

        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(jwtConfig)
            .Build();

        _authService = new AuthService(
            _userRepositoryMock.Object,
            _workspaceMembershipRepositoryMock.Object,
            _teacherRepositoryMock.Object,
            _studentRepositoryMock.Object,
            _schoolRepositoryMock.Object,
            configuration
        );
    }

    [Fact]
    public void HashPassword_ShouldReturnDifferentHash_ForSamePassword()
    {
        // Arrange
        var password = "TestPassword123!";

        // Act
        var hash1 = _authService.HashPassword(password);
        var hash2 = _authService.HashPassword(password);

        // Assert
        Assert.NotEqual(hash1, hash2);
    }

    [Fact]
    public void VerifyPassword_ShouldReturnTrue_ForCorrectPassword()
    {
        // Arrange
        var password = "TestPassword123!";
        var hash = _authService.HashPassword(password);

        // Act
        var result = _authService.VerifyPassword(password, hash);

        // Assert
        Assert.True(result);
    }

    [Fact]
    public void VerifyPassword_ShouldReturnFalse_ForIncorrectPassword()
    {
        // Arrange
        var password = "TestPassword123!";
        var wrongPassword = "WrongPassword456!";
        var hash = _authService.HashPassword(password);

        // Act
        var result = _authService.VerifyPassword(wrongPassword, hash);

        // Assert
        Assert.False(result);
    }

    [Fact]
    public async Task LoginAsync_ShouldReturnFailed_WhenUserNotFound()
    {
        // Arrange
        var email = "nonexistent@example.com";
        var password = "Password123!";

        _userRepositoryMock
            .Setup(x => x.GetByEmailAsync(email))
            .ReturnsAsync((UserEntity)null!);

        // Act
        var result = await _authService.LoginAsync(email, password);

        // Assert
        Assert.False(result.Success);
        Assert.Contains("Invalid", result.Message);
    }

    [Fact]
    public async Task LoginAsync_ShouldReturnFailed_WhenPasswordIsIncorrect()
    {
        // Arrange
        var email = "test@example.com";
        var password = "CorrectPassword123!";
        var wrongPassword = "WrongPassword456!";

        var user = new UserEntity
        {
            Email = email,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(password),
            Role = UserRole.Teacher,
            Status = AccountStatus.Active
        };

        _userRepositoryMock
            .Setup(x => x.GetByEmailAsync(email))
            .ReturnsAsync(user);

        // Act
        var result = await _authService.LoginAsync(email, wrongPassword);

        // Assert
        Assert.False(result.Success);
    }

    [Fact]
    public async Task LoginAsync_ShouldReturnSuccess_WhenCredentialsAreCorrect()
    {
        // Arrange
        var email = "test@example.com";
        var password = "TestPassword123!";

        var user = new UserEntity
        {
            Email = email,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(password),
            Role = UserRole.Teacher,
            Status = AccountStatus.Active
        };

        _userRepositoryMock
            .Setup(x => x.GetByEmailAsync(email))
            .ReturnsAsync(user);

        // Act
        var result = await _authService.LoginAsync(email, password);

        // Assert
        Assert.True(result.Success);
        Assert.NotNull(result.User);
        Assert.Equal(email, result.User.Email);
    }

    [Fact]
    public async Task RegisterStudentAsync_ShouldReturnFailed_WhenEmailAlreadyExists()
    {
        // Arrange
        var email = "existing@example.com";
        var password = "StudentPassword123!";

        var existingUser = new UserEntity
        {
            Email = email,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(password),
            Role = UserRole.Student,
            Status = AccountStatus.Active
        };

        _userRepositoryMock
            .Setup(x => x.GetByEmailAsync(email))
            .ReturnsAsync(existingUser);

        // Act
        var (success, message) = await _authService.RegisterStudentAsync(
            email,
            password,
            "John",
            "Doe"
        );

        // Assert
        Assert.False(success);
        Assert.Contains("already", message.ToLower());
    }

    [Fact]
    public async Task RegisterStudentAsync_ShouldReturnFailed_WhenPasswordTooShort()
    {
        // Arrange
        var email = "student@example.com";
        var password = "short";

        _userRepositoryMock
            .Setup(x => x.GetByEmailAsync(email))
            .ReturnsAsync((UserEntity)null!);

        // Act
        var (success, message) = await _authService.RegisterStudentAsync(
            email,
            password,
            "John",
            "Doe"
        );

        // Assert
        Assert.False(success);
        Assert.Contains("6 characters", message);
    }

    [Fact]
    public async Task RegisterStudentAsync_ShouldReturnSuccess_ForValidInput()
    {
        // Arrange
        var email = "student@example.com";
        var password = "ValidPassword123!";

        _userRepositoryMock
            .Setup(x => x.GetByEmailAsync(email))
            .ReturnsAsync((UserEntity)null!);

        var createdUser = new UserEntity 
        { 
            Email = email, 
            PasswordHash = _authService.HashPassword(password),
            Role = UserRole.Student,
            Status = AccountStatus.Pending
        };

        _userRepositoryMock
            .Setup(x => x.CreateAsync(It.IsAny<UserEntity>()))
            .ReturnsAsync(createdUser);

        _studentRepositoryMock
            .Setup(x => x.CreateAsync(It.IsAny<StudentEntity>()))
            .ReturnsAsync((StudentEntity s) => s);

        // Act
        var (success, message) = await _authService.RegisterStudentAsync(
            email,
            password,
            "John",
            "Doe"
        );

        // Assert
        Assert.True(success);
        Assert.Contains("successful", message.ToLower());
        _userRepositoryMock.Verify(x => x.CreateAsync(It.IsAny<UserEntity>()), Times.Once);
        _studentRepositoryMock.Verify(x => x.CreateAsync(It.IsAny<StudentEntity>()), Times.Once);
    }

    [Fact]
    public async Task RegisterStandaloneTeacherAsync_ShouldReturnFailed_WhenEmailAlreadyExists()
    {
        // Arrange
        var email = "teacher@example.com";
        var password = "TeacherPassword123!";

        var existingUser = new UserEntity
        {
            Email = email,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(password),
            Role = UserRole.Teacher,
            Status = AccountStatus.Active
        };

        _userRepositoryMock
            .Setup(x => x.GetByEmailAsync(email))
            .ReturnsAsync(existingUser);

        // Act
        var (success, message) = await _authService.RegisterStandaloneTeacherAsync(
            email,
            password,
            "Jane",
            "Doe"
        );

        // Assert
        Assert.False(success);
        Assert.Contains("already", message.ToLower());
    }

    [Fact]
    public async Task RegisterStandaloneTeacherAsync_ShouldReturnSuccess_ForValidInput()
    {
        // Arrange
        var email = "teacher@example.com";
        var password = "TeacherPassword123!";

        _userRepositoryMock
            .Setup(x => x.GetByEmailAsync(email))
            .ReturnsAsync((UserEntity)null!);

        var createdUser = new UserEntity 
        { 
            Email = email, 
            PasswordHash = _authService.HashPassword(password),
            Role = UserRole.Teacher,
            Status = AccountStatus.Active
        };

        _userRepositoryMock
            .Setup(x => x.CreateAsync(It.IsAny<UserEntity>()))
            .ReturnsAsync(createdUser);

        _teacherRepositoryMock
            .Setup(x => x.CreateAsync(It.IsAny<TeacherEntity>()))
            .ReturnsAsync((TeacherEntity t) => t);

        _workspaceMembershipRepositoryMock
            .Setup(x => x.CreateAsync(It.IsAny<WorkspaceMembershipEntity>()))
            .ReturnsAsync((WorkspaceMembershipEntity w) => w);

        // Act
        var (success, message) = await _authService.RegisterStandaloneTeacherAsync(
            email,
            password,
            "Jane",
            "Doe"
        );

        // Assert
        Assert.True(success);
        Assert.Contains("successful", message.ToLower());
        _userRepositoryMock.Verify(x => x.CreateAsync(It.IsAny<UserEntity>()), Times.Once);
        _teacherRepositoryMock.Verify(x => x.CreateAsync(It.IsAny<TeacherEntity>()), Times.Once);
    }

    [Fact]
    public async Task GenerateJwtTokenAsync_ShouldReturnValidToken()
    {
        // Arrange
        var user = new UserEntity
        {
            Email = "test@example.com",
            PasswordHash = "hash",
            Role = UserRole.Teacher,
            Status = AccountStatus.Active
        };

        // Act
        var token = await _authService.GenerateJwtTokenAsync(user, null);

        // Assert
        Assert.NotNull(token);
        Assert.NotEmpty(token);
        Assert.Contains(".", token); // JWT tokens have dots separating parts
    }
}
