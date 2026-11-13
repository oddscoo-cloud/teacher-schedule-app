namespace backend.Core.Interfaces.Services;

using backend.Core.Entities;

public class LoginResult
{
    public bool Success { get; set; }
    public string? Message { get; set; }
    public UserEntity? User { get; set; }
}

public interface IAuthService
{
    Task<LoginResult> LoginAsync(string email, string password);
    Task<(bool success, string message)> RegisterStudentAsync(string email, string password, string firstName, string lastName);
    Task<(bool success, string message)> RegisterStandaloneTeacherAsync(string email, string password, string firstName, string lastName);
    Task<(bool success, string message)> InviteSchoolTeacherAsync(string email, int schoolId);
    Task<(bool success, string message)> ConfirmStudentAsync(int studentId, int? teacherId, int? schoolId);
    Task<string> GenerateJwtTokenAsync(UserEntity user, int? workspaceMembershipId = null);
    string HashPassword(string password);
    bool VerifyPassword(string password, string hash);
}
