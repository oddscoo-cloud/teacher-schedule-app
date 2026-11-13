namespace backend.Core.Entities;

public class WorkspaceMembershipEntity
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public int? SchoolId { get; set; }
    public WorkspaceType WorkspaceType { get; set; }
    public bool IsDefault { get; set; }
    public DateTime JoinedAt { get; set; }

    // Связи
    public UserEntity User { get; set; } = null!;
    public SchoolEntity? School { get; set; }
}
