namespace backend.Presentation.Controllers;

using backend.Core.Entities;
using backend.Core.Interfaces.Services;
using backend.Presentation.DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

[ApiController]
[Route("api/students")]
[Authorize]
public class StudentsController : AuthorizedWorkspaceControllerBase
{
    private readonly IStudentService _students;

    public StudentsController(IStudentService students)
    {
        _students = students;
    }

    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<StudentResponseDto>>> GetStudents(
        [FromQuery] string? search,
        [FromQuery] int? course,
        [FromQuery] string? status)
    {
        if (!TryGetRequester(out var requester))
            return Unauthorized();

        AccountStatus? parsedStatus = null;
        if (!string.IsNullOrWhiteSpace(status))
        {
            if (!Enum.TryParse<AccountStatus>(status, true, out var value) || !Enum.IsDefined(value))
                return BadRequest(new { message = "Unknown student status." });
            parsedStatus = value;
        }

        var students = await _students.GetStudentsAsync(
            requester.UserId,
            requester.Role,
            requester.MembershipId,
            search,
            course,
            parsedStatus);
        if (students == null)
            return Forbid();

        return Ok(students.Select(ToResponse));
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<StudentResponseDto>> GetStudent(int id)
    {
        if (!TryGetRequester(out var requester))
            return Unauthorized();

        var student = await _students.GetStudentAsync(
            requester.UserId,
            requester.Role,
            requester.MembershipId,
            id);
        return student == null ? NotFound() : Ok(ToResponse(student));
    }

    [HttpPost]
    public async Task<ActionResult<StudentResponseDto>> CreateStudent(StudentRequestDto request)
    {
        if (!TryGetRequester(out var requester))
            return Unauthorized();

        var student = await _students.CreateStudentAsync(
            requester.UserId,
            requester.Role,
            requester.MembershipId,
            ToEntity(request),
            request.CourseIds,
            ToContacts(request));
        if (student == null)
            return Forbid();

        return CreatedAtAction(nameof(GetStudent), new { id = student.Id }, ToResponse(student));
    }

    [HttpPut("{id:int}")]
    public async Task<ActionResult<StudentResponseDto>> UpdateStudent(int id, StudentRequestDto request)
    {
        if (!TryGetRequester(out var requester))
            return Unauthorized();

        var student = await _students.UpdateStudentAsync(
            requester.UserId,
            requester.Role,
            requester.MembershipId,
            id,
            ToEntity(request),
            request.CourseIds,
            ToContacts(request));
        return student == null ? NotFound() : Ok(ToResponse(student));
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> DeleteStudent(int id)
    {
        if (!TryGetRequester(out var requester))
            return Unauthorized();

        var deleted = await _students.DeleteStudentAsync(
            requester.UserId,
            requester.Role,
            requester.MembershipId,
            id);
        return deleted ? NoContent() : NotFound();
    }

    private static StudentEntity ToEntity(StudentRequestDto request) => new()
    {
        FirstName = request.FirstName.Trim(),
        LastName = request.LastName.Trim(),
        Patronymic = request.Patronymic?.Trim(),
        Email = request.Email?.Trim(),
        PhoneNumber = request.PhoneNumber?.Trim(),
        DateOfBirth = request.DateOfBirth
    };

    private static IReadOnlyCollection<StudentContactEntity> ToContacts(StudentRequestDto request) =>
        request.Contacts.Select(contact => new StudentContactEntity
        {
            Relationship = contact.Relationship?.Trim(),
            Contact = new ContactEntity
            {
                FirstName = contact.FirstName.Trim(),
                LastName = contact.LastName.Trim(),
                Email = contact.Email?.Trim(),
                PhoneNumber = contact.PhoneNumber?.Trim()
            }
        }).ToList();

    private static StudentResponseDto ToResponse(StudentEntity student) => new()
    {
        Id = student.Id,
        FirstName = student.FirstName,
        LastName = student.LastName,
        Patronymic = student.Patronymic,
        Email = student.Email ?? student.User?.Email,
        PhoneNumber = student.PhoneNumber,
        DateOfBirth = student.DateOfBirth,
        Status = student.Status.ToString(),
        Contacts = student.Contacts.Select(link => new StudentContactResponseDto
        {
            FirstName = link.Contact.FirstName,
            LastName = link.Contact.LastName,
            Email = link.Contact.Email,
            PhoneNumber = link.Contact.PhoneNumber,
            Relationship = link.Relationship
        }).ToList(),
        Courses = student.CourseAssignments.Select(link => new CourseResponseDto
        {
            Id = link.Course.Id,
            Name = link.Course.Name,
            Description = link.Course.Description
        }).ToList(),
        Notes = student.Notes.Select(note => new StudentNoteResponseDto
        {
            Id = note.Id,
            TeacherId = note.TeacherId,
            Content = note.Content,
            Goals = note.Goals,
            Progress = note.Progress,
            CreatedAt = note.CreatedAt,
            UpdatedAt = note.UpdatedAt
        }).ToList(),
        Assignments = student.TeacherAssignments.Select(assignment => new StudentAssignmentResponseDto
        {
            TeacherId = assignment.TeacherId,
            TeacherName = assignment.Teacher.FirstName + " " + assignment.Teacher.LastName,
            TeacherEmail = assignment.Teacher.User.Email,
            AssignedAt = assignment.AssignedAt
        }).ToList()
    };
}
