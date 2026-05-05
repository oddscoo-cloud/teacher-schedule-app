using Microsoft.AspNetCore.Mvc;
using backend.Data;
using System.Linq;
using backend.Data;
using backend.Models;
using System.Text;
using System.IdentityModel.Tokens.Jwt;
using Microsoft.IdentityModel.Tokens;
using System.Security.Claims;

[ApiController]
[Route("api/auth")]
public class AuthController : ControllerBase
{
    private readonly AppDbContext _context;

    public AuthController(AppDbContext context)
    {
        _context = context;
    }

    [HttpPost("login")]
    public IActionResult Login(LoginRequest request)
    {
        var user = _context.Users
            .FirstOrDefault(u => u.Login == request.Login && u.Password == request.Password);

        if (user == null)
            return Unauthorized();

        var claims = new[]
        {
        new Claim(ClaimTypes.Name, user.Login),
        new Claim(ClaimTypes.Role, user.Role),
        new Claim("teacherId", user.TeacherId?.ToString() ?? "")
    };

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes("teacher_schedule_super_secret_key_bY85S96q1"));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            claims: claims,
            expires: DateTime.Now.AddHours(2),
            signingCredentials: creds
        );

        return Ok(new
        {
            token = new JwtSecurityTokenHandler().WriteToken(token),
            role = user.Role,
            teacherId = user.TeacherId
        });
    }
}