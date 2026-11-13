namespace backend.Presentation.DTOs;

public class LoginRequestDto
{
    public string Email { get; set; } = null!;
    public string Password { get; set; } = null!;
}

public class LoginResponseDto
{
    public bool Success { get; set; }
    public string? Message { get; set; }
    public string? Token { get; set; }
    public UserDto? User { get; set; }
    public IEnumerable<WorkspaceDto>? Workspaces { get; set; }
}

public class UserDto
{
    public int Id { get; set; }
    public string Email { get; set; } = null!;
    public string Role { get; set; } = null!;
    public string Status { get; set; } = null!;
}

public class WorkspaceDto
{
    public int Id { get; set; }
    public string Type { get; set; } = null!;
    public int? SchoolId { get; set; }
    public string? SchoolName { get; set; }
    public bool IsDefault { get; set; }
}

public class RegisterRequestDto
{
    public string Email { get; set; } = null!;
    public string Password { get; set; } = null!;
    public string FirstName { get; set; } = null!;
    public string LastName { get; set; } = null!;
    public string UserType { get; set; } = null!; // "student" или "standalone-teacher"
}

public class RegisterResponseDto
{
    public bool Success { get; set; }
    public string Message { get; set; } = null!;
    public UserDto? User { get; set; }
}

public class InviteTeacherRequestDto
{
    public string Email { get; set; } = null!;
}

public class ConfirmStudentRequestDto
{
    public int TeacherId { get; set; }
}
