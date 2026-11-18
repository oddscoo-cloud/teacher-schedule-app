namespace backend.Core.Entities;

public class ContactEntity
{
    public int Id { get; set; }
    public string FirstName { get; set; } = null!;
    public string LastName { get; set; } = null!;
    public string? Email { get; set; }
    public string? PhoneNumber { get; set; }
    public ICollection<StudentContactEntity> Students { get; set; } = new List<StudentContactEntity>();
}
