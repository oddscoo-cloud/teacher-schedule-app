namespace backend.Presentation.Controllers;

using backend.Core.Interfaces.Services;
using backend.Presentation.DTOs;
using Microsoft.AspNetCore.Mvc;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly IAuthService _authService;
    private readonly IWorkspaceService _workspaceService;
    private readonly IConfiguration _configuration;

    public AuthController(
        IAuthService authService,
        IWorkspaceService workspaceService,
        IConfiguration configuration)
    {
        _authService = authService;
        _workspaceService = workspaceService;
        _configuration = configuration;
    }

    [HttpPost("login")]
    public async Task<ActionResult<LoginResponseDto>> Login([FromBody] LoginRequestDto request)
    {
        if (!ModelState.IsValid)
            return BadRequest(new LoginResponseDto { Success = false, Message = "Invalid request" });

        var result = await _authService.LoginAsync(request.Email, request.Password);

        if (!result.Success)
            return Unauthorized(new LoginResponseDto { Success = false, Message = result.Message });

        var user = result.User!;
        var workspaces = await _workspaceService.GetUserWorkspacesAsync(user.Id);
        var defaultWorkspace = workspaces.FirstOrDefault(w => w.IsDefault);

        var token = await _authService.GenerateJwtTokenAsync(user, defaultWorkspace?.Id);

        var workspaceDtos = workspaces.Select(w => new WorkspaceDto
        {
            Id = w.Id,
            Type = w.WorkspaceType.ToString(),
            SchoolId = w.SchoolId,
            SchoolName = w.School?.Name,
            IsDefault = w.IsDefault
        }).ToList();

        var userDto = new UserDto
        {
            Id = user.Id,
            Email = user.Email,
            Role = user.Role.ToString(),
            Status = user.Status.ToString()
        };

        return Ok(new LoginResponseDto
        {
            Success = true,
            Token = token,
            User = userDto,
            Workspaces = workspaceDtos
        });
    }

    [HttpPost("register/student")]
    public async Task<ActionResult<RegisterResponseDto>> RegisterStudent([FromBody] RegisterRequestDto request)
    {
        if (!ModelState.IsValid)
            return BadRequest(new RegisterResponseDto { Success = false, Message = "Invalid request" });

        var (success, message) = await _authService.RegisterStudentAsync(
            request.Email,
            request.Password,
            request.FirstName,
            request.LastName);

        if (!success)
            return BadRequest(new RegisterResponseDto { Success = false, Message = message });

        return Ok(new RegisterResponseDto
        {
            Success = true,
            Message = message
        });
    }

    [HttpPost("register/teacher")]
    public async Task<ActionResult<RegisterResponseDto>> RegisterTeacher([FromBody] RegisterRequestDto request)
    {
        if (!ModelState.IsValid)
            return BadRequest(new RegisterResponseDto { Success = false, Message = "Invalid request" });

        var (success, message) = await _authService.RegisterStandaloneTeacherAsync(
            request.Email,
            request.Password,
            request.FirstName,
            request.LastName);

        if (!success)
            return BadRequest(new RegisterResponseDto { Success = false, Message = message });

        return Ok(new RegisterResponseDto
        {
            Success = true,
            Message = message
        });
    }
}
