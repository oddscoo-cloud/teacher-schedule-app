namespace backend.Presentation.Controllers;

using backend.Core.Use_Cases.Auth;
using backend.Core.Interfaces.Repositories;
using backend.Core.Interfaces.Services;
using backend.Presentation.DTOs;
using Microsoft.AspNetCore.Mvc;

[ApiController]
[Route("api/[controller]")]
public class BootstrapController : ControllerBase
{
    private readonly IConfiguration _configuration;
    private readonly IUserRepository _userRepository;
    private readonly ISchoolRepository _schoolRepository;
    private readonly IWorkspaceMembershipRepository _membershipRepository;
    private readonly IAuthService _authService;

    public BootstrapController(
        IConfiguration configuration,
        IUserRepository userRepository,
        ISchoolRepository schoolRepository,
        IWorkspaceMembershipRepository membershipRepository,
        IAuthService authService)
    {
        _configuration = configuration;
        _userRepository = userRepository;
        _schoolRepository = schoolRepository;
        _membershipRepository = membershipRepository;
        _authService = authService;
    }

    [HttpPost("create-school-admin")]
    public async Task<ActionResult<object>> CreateSchoolAdmin([FromBody] CreateSchoolAdminRequestDto request)
    {
        if (!ModelState.IsValid)
            return BadRequest(new { success = false, message = "Invalid request" });

        var bootstrapSecret = _configuration["Bootstrap:SecretKey"];
        if (string.IsNullOrWhiteSpace(bootstrapSecret) || request.BootstrapSecret != bootstrapSecret)
            return Unauthorized(new { success = false, message = "Invalid bootstrap secret" });

        var useCase = new CreateSchoolAdminUseCase(
            _userRepository,
            _schoolRepository,
            _membershipRepository,
            _authService);

        var (success, message, schoolId, userId) = await useCase.ExecuteAsync(
            request.AdminEmail,
            request.AdminPassword,
            request.SchoolName,
            request.BootstrapSecret);

        if (!success)
            return BadRequest(new { success = false, message });

        return Ok(new
        {
            success = true,
            message,
            schoolId,
            userId,
            note = "Bootstrap is a one-time operation. This endpoint is disabled after the first successful use."
        });
    }

    [HttpGet("status")]
    public async Task<ActionResult<object>> GetBootstrapStatus()
    {
        var school = await _schoolRepository.GetDefaultSchoolAsync();
        var isBootstrapped = school != null;

        return Ok(new
        {
            bootstrapped = isBootstrapped,
            message = isBootstrapped
                ? "System is already bootstrapped"
                : "System needs bootstrap. Use POST /api/bootstrap/create-school-admin with bootstrap secret."
        });
    }
}

public class CreateSchoolAdminRequestDto
{
    public string AdminEmail { get; set; } = null!;
    public string AdminPassword { get; set; } = null!;
    public string SchoolName { get; set; } = null!;
    public string BootstrapSecret { get; set; } = null!;
}
