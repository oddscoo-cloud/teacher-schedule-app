namespace backend.Presentation.DTOs;

using System.ComponentModel.DataAnnotations;

public class StudentRequestDto
{
    [Required, StringLength(255)]
    public string FirstName { get; set; } = null!;

    [Required, StringLength(255)]
    public string LastName { get; set; } = null!;

    [StringLength(255)]
    public string? Patronymic { get; set; }

    [EmailAddress, StringLength(255)]
    public string? Email { get; set; }

    [Phone, StringLength(50)]
    public string? PhoneNumber { get; set; }

    public DateTime? DateOfBirth { get; set; }
    public List<int> CourseIds { get; set; } = new();
    public List<StudentContactRequestDto> Contacts { get; set; } = new();
}

public class StudentContactRequestDto
{
    [Required, StringLength(255)]
    public string FirstName { get; set; } = null!;

    [Required, StringLength(255)]
    public string LastName { get; set; } = null!;

    [EmailAddress, StringLength(255)]
    public string? Email { get; set; }

    [Phone, StringLength(50)]
    public string? PhoneNumber { get; set; }

    [StringLength(100)]
    public string? Relationship { get; set; }
}

public class StudentResponseDto
{
    public int Id { get; set; }
    public string FirstName { get; set; } = null!;
    public string LastName { get; set; } = null!;
    public string? Patronymic { get; set; }
    public string? Email { get; set; }
    public string? PhoneNumber { get; set; }
    public DateTime? DateOfBirth { get; set; }
    public string Status { get; set; } = null!;
    public List<StudentContactResponseDto> Contacts { get; set; } = new();
    public List<CourseResponseDto> Courses { get; set; } = new();
    public List<StudentNoteResponseDto> Notes { get; set; } = new();
    public List<StudentAssignmentResponseDto> Assignments { get; set; } = new();
}

public class StudentContactResponseDto
{
    public string FirstName { get; set; } = null!;
    public string LastName { get; set; } = null!;
    public string? Email { get; set; }
    public string? PhoneNumber { get; set; }
    public string? Relationship { get; set; }
}

public class CourseRequestDto
{
    [Required, StringLength(255)]
    public string Name { get; set; } = null!;

    public string? Description { get; set; }
}

public class CourseResponseDto
{
    public int Id { get; set; }
    public string Name { get; set; } = null!;
    public string? Description { get; set; }
}

public class StudentNoteRequestDto
{
    [Required]
    public string Content { get; set; } = null!;

    public string? Goals { get; set; }
    public string? Progress { get; set; }
}

public class StudentNoteResponseDto
{
    public int Id { get; set; }
    public int TeacherId { get; set; }
    public string Content { get; set; } = null!;
    public string? Goals { get; set; }
    public string? Progress { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}

public class StudentAssignmentResponseDto
{
    public int TeacherId { get; set; }
    public string TeacherName { get; set; } = null!;
    public string TeacherEmail { get; set; } = null!;
    public DateTime AssignedAt { get; set; }
}
