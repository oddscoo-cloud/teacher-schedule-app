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

    public TeacherController(
        ITeacherRepository teacherRepository,
        IStudentRepository studentRepository,
        IUserRepository userRepository,
        IWorkspaceMembershipRepository membershipRepository)
    {
        _teacherRepository = teacherRepository;
        _studentRepository = studentRepository;
        _userRepository = userRepository;
        _membershipRepository = membershipRepository;
    }

    [HttpPost("confirm-student")]
    public async Task<ActionResult<object>> ConfirmStudent([FromBody] ConfirmStandaloneStudentDto request)
    {
        if (!ModelState.IsValid)
            return BadRequest(new { success = false, message = "Invalid request" });

        var userId = GetUserId();
        if (userId == null)
            return Unauthorized();

        var teacher = await _teacherRepository.GetByUserIdAsync(userId.Value);
        if (teacher == null)
            return Unauthorized(new { success = false, message = "User is not a teacher" });

        // Only standalone teachers can confirm students this way
        if (teacher.SchoolId != null)
            return StatusCode(StatusCodes.Status403Forbidden, new { success = false, message = "Use school admin endpoints for school students" });

        var useCase = new ConfirmStudentUseCase(
            _studentRepository,
            _teacherRepository,
            null!, // schoolRepository - not used for standalone
            _userRepository);

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

        var teacher = await _teacherRepository.GetByUserIdAsync(userId.Value);
        if (teacher == null)
            return Unauthorized();

        var students = await _studentRepository.GetByTeacherIdAsync(teacher.Id);

        var result = students.Select(s => new
        {
            id = s.Id,
            firstName = s.FirstName,
            lastName = s.LastName,
            email = s.User.Email,
            status = s.Status.ToString(),
            phoneNumber = s.PhoneNumber,
            isStandalone = s.StandaloneTeacherId == teacher.Id
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
