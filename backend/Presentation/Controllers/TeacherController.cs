namespace backend.Presentation.Controllers;

using backend.Core.Entities;
using backend.Core.Interfaces.Repositories;
using backend.Core.Interfaces.Services;
using backend.Core.Use_Cases.Auth;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = "Teacher")]
public class TeacherController : ControllerBase
{
    private readonly ITeacherRepository _teacherRepository;
    private readonly IStudentRepository _studentRepository;
    private readonly IUserRepository _userRepository;
    private readonly IWorkspaceMembershipRepository _membershipRepository;
    private readonly IStudentService _studentService;

    public TeacherController(
        ITeacherRepository teacherRepository,
        IStudentRepository studentRepository,
        IUserRepository userRepository,
        IWorkspaceMembershipRepository membershipRepository,
        IStudentService studentService)
    {
        _teacherRepository = teacherRepository;
        _studentRepository = studentRepository;
        _userRepository = userRepository;
        _membershipRepository = membershipRepository;
        _studentService = studentService;
    }

    [HttpPost("confirm-student")]
    public async Task<ActionResult<object>> ConfirmStudent([FromBody] ConfirmStandaloneStudentDto request)
    {
        if (!ModelState.IsValid)
            return BadRequest(new { success = false, message = "Invalid request" });

        var userId = GetUserId();
        if (userId == null)
            return Unauthorized();

        if (!int.TryParse(User.FindFirst("workspaceMembershipId")?.Value, out var membershipId))
            return Unauthorized();

        var membership = await _membershipRepository.GetByIdAsync(membershipId);
        if (membership == null ||
            membership.UserId != userId.Value ||
            membership.WorkspaceType != WorkspaceType.Standalone)
            return Forbid();

        var teacher = await _teacherRepository.GetByUserIdAndSchoolIdAsync(userId.Value, null);
        if (teacher == null)
            return Unauthorized(new { success = false, message = "User is not a teacher" });

        // Only standalone teachers can confirm students this way
        if (teacher.SchoolId != null)
            return StatusCode(StatusCodes.Status403Forbidden, new { success = false, message = "Use school admin endpoints for school students" });

        var useCase = new ConfirmStudentUseCase(
            _studentRepository,
            _teacherRepository,
            null!, // schoolRepository - not used for standalone
            _userRepository,
            _membershipRepository);

        var (success, message) = await useCase.ExecuteAsync(
            request.StudentId,
            schoolId: null,
            teacherId: teacher.Id);

        if (!success)
            return BadRequest(new { success = false, message });

        return Ok(new { success = true, message });
    }

    [HttpGet("students")]
    public async Task<ActionResult<IEnumerable<object>>> GetStudents()
    {
        var userId = GetUserId();
        if (userId == null)
            return Unauthorized();

        if (!int.TryParse(User.FindFirst("workspaceMembershipId")?.Value, out var membershipId))
            return Unauthorized();

        var students = await _studentService.GetStudentsAsync(
            userId.Value,
            UserRole.Teacher,
            membershipId,
            null,
            null,
            null);
        if (students == null)
            return Forbid();

        var result = students.Select(s => new
        {
            id = s.Id,
            firstName = s.FirstName,
            lastName = s.LastName,
            email = s.Email ?? s.User?.Email,
            status = s.Status.ToString(),
            phoneNumber = s.PhoneNumber,
        });

        return Ok(result);
    }

    private int? GetUserId()
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
        if (userIdClaim == null || !int.TryParse(userIdClaim.Value, out var userId))
            return null;

        return userId;
    }
}

public class ConfirmStandaloneStudentDto
{
    public int StudentId { get; set; }
}
