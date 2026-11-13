namespace backend.Core.Entities;

public class SchoolEntity
{
    public int Id { get; set; }
    public string Name { get; set; } = null!;
    public string? Description { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }

    // Связи
    public ICollection<WorkspaceMembershipEntity> Memberships { get; set; } = new List<WorkspaceMembershipEntity>();
    public ICollection<StudentEntity> Students { get; set; } = new List<StudentEntity>();
}
