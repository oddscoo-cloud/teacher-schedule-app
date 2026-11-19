namespace backend.Presentation.Controllers;

using backend.Core.Interfaces.Repositories;
using backend.Core.Interfaces.Services;
using backend.Core.Use_Cases.Auth;
using backend.Presentation.DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = "SchoolAdmin")]
public class SchoolAdminController : ControllerBase
{
    private readonly ISchoolRepository _schoolRepository;
    private readonly ITeacherRepository _teacherRepository;
    private readonly IStudentRepository _studentRepository;
    private readonly IWorkspaceMembershipRepository _membershipRepository;
    private readonly IWorkspaceService _workspaceService;
    private readonly IAuthService _authService;
    private readonly IUserRepository _userRepository;

    public SchoolAdminController(
        ISchoolRepository schoolRepository,
        ITeacherRepository teacherRepository,
        IStudentRepository studentRepository,
        IWorkspaceMembershipRepository membershipRepository,
        IWorkspaceService workspaceService,
        IAuthService authService,
        IUserRepository userRepository)
    {
        _schoolRepository = schoolRepository;
        _teacherRepository = teacherRepository;
        _studentRepository = studentRepository;
        _membershipRepository = membershipRepository;
        _workspaceService = workspaceService;
        _authService = authService;
        _userRepository = userRepository;
    }

    [HttpPost("invite-teacher")]
    public async Task<ActionResult<object>> InviteTeacher([FromBody] InviteTeacherRequestDto request)
    {
        if (!ModelState.IsValid)
            return BadRequest(new { success = false, message = "Invalid request" });

        var schoolId = await GetAdminSchoolIdAsync();
        if (schoolId == null)
            return Unauthorized(new { success = false, message = "Not a school admin" });

        var useCase = new InviteSchoolTeacherUseCase(
            _userRepository,
            _membershipRepository,
            _schoolRepository,
            _teacherRepository,
            _authService);

        var (success, message) = await useCase.ExecuteAsync(request.Email, schoolId.Value);

        if (!success)
            return BadRequest(new { success = false, message });

        return Ok(new { success = true, message });
    }

    [HttpPost("confirm-student")]
    public async Task<ActionResult<object>> ConfirmStudent([FromBody] ConfirmStudentDto request)
    {
        if (!ModelState.IsValid)
            return BadRequest(new { success = false, message = "Invalid request" });

        var schoolId = await GetAdminSchoolIdAsync();
        if (schoolId == null)
            return Unauthorized(new { success = false, message = "Not a school admin" });

        var useCase = new ConfirmStudentUseCase(
            _studentRepository,
            _teacherRepository,
            _schoolRepository,
            _userRepository,
            _membershipRepository);

        var (success, message) = await useCase.ExecuteAsync(
            request.StudentId,
            schoolId: schoolId.Value,
            teacherId: request.TeacherId);

        if (!success)
            return BadRequest(new { success = false, message });

        return Ok(new { success = true, message });
    }

    [HttpGet("students")]
    public async Task<ActionResult<IEnumerable<object>>> GetStudents()
    {
        var schoolId = await GetAdminSchoolIdAsync();
        if (schoolId == null)
            return Unauthorized();

        var students = await _studentRepository.GetBySchoolIdAsync(schoolId.Value);

        var result = students.Select(s => new
        {
            id = s.Id,
            firstName = s.FirstName,
            lastName = s.LastName,
            email = s.Email ?? s.User?.Email,
            status = s.Status.ToString(),
            phoneNumber = s.PhoneNumber,
            teachers = s.TeacherAssignments.Select(ta => new { id = ta.TeacherId, name = ta.Teacher.FirstName + " " + ta.Teacher.LastName })
        });

        return Ok(result);
    }

    [HttpGet("teachers")]
    public async Task<ActionResult<IEnumerable<object>>> GetTeachers()
    {
        var schoolId = await GetAdminSchoolIdAsync();
        if (schoolId == null)
            return Unauthorized();

        var teachers = await _teacherRepository.GetBySchoolIdAsync(schoolId.Value);

        var result = teachers.Select(t => new
        {
            id = t.Id,
            firstName = t.FirstName,
            lastName = t.LastName,
            email = t.User.Email,
            bio = t.Bio,
            studentCount = t.StudentAssignments.Count()
        });

        return Ok(result);
    }

    private async Task<int?> GetAdminSchoolIdAsync()
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
        if (userIdClaim == null || !int.TryParse(userIdClaim.Value, out var userId))
            return null;

        var memberships = await _membershipRepository.GetByUserIdAsync(userId);
        var schoolMembership = memberships.FirstOrDefault(m => m.WorkspaceType == Core.Entities.WorkspaceType.School);

        return schoolMembership?.SchoolId;
    }
}

public class ConfirmStudentDto
{
    public int StudentId { get; set; }
    public int? TeacherId { get; set; }
}
