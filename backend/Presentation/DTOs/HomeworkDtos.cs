namespace backend.Presentation.DTOs;

using System.ComponentModel.DataAnnotations;

public class HomeworkCreateRequestDto
{
    [Range(1, int.MaxValue)]
    public int StudentId { get; set; }

    public int? CourseId { get; set; }

    [Required, StringLength(255)]
    public string Title { get; set; } = null!;

    [StringLength(4000)]
    public string? Instructions { get; set; }

    public DateTime? DueAt { get; set; }
}

public class HomeworkGradeRequestDto
{
    [Range(0, 100)]
    public int Grade { get; set; }
}

public class HomeworkResponseDto
{
    public int Id { get; set; }
    public int StudentId { get; set; }
    public string StudentName { get; set; } = null!;
    public int TeacherId { get; set; }
    public string TeacherName { get; set; } = null!;
    public int? CourseId { get; set; }
    public string? CourseName { get; set; }
    public string Title { get; set; } = null!;
    public string? Instructions { get; set; }
    public DateTime? DueAt { get; set; }
    public DateTime CreatedAt { get; set; }
    public List<HomeworkSubmissionResponseDto> Submissions { get; set; } = new();
}

public class HomeworkSubmissionResponseDto
{
    public int Id { get; set; }
    public int AttemptNumber { get; set; }
    public string? Text { get; set; }
    public List<string> Links { get; set; } = new();
    public int? Grade { get; set; }
    public DateTime SubmittedAt { get; set; }
    public DateTime? GradedAt { get; set; }
    public List<HomeworkAttachmentResponseDto> Attachments { get; set; } = new();
}

public class HomeworkAttachmentResponseDto
{
    public int Id { get; set; }
    public string DisplayName { get; set; } = null!;
    public string ContentType { get; set; } = null!;
    public long SizeBytes { get; set; }
    public string DownloadUrl { get; set; } = null!;
}
