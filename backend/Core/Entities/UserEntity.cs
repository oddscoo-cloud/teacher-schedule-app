namespace backend.Core.Entities;

public enum UserRole
{
    SchoolAdmin,
    Teacher,
    Student
}

public enum AccountStatus
{
    Active,
    Pending,
    Disabled
}

public enum WorkspaceType
{
    School,
    Standalone
}

public class UserEntity
{
    public int Id { get; set; }
    public string Email { get; set; } = null!;
    public string PasswordHash { get; set; } = null!;
    public UserRole Role { get; set; }
    public AccountStatus Status { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }

    // Связи
    public ICollection<WorkspaceMembershipEntity> WorkspaceMemberships { get; set; } = new List<WorkspaceMembershipEntity>();
}
