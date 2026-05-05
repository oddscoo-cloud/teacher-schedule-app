using backend.Data;
using backend.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;

namespace backend.Controllers;

[ApiController]
[Route("api/[controller]")]
public class TeachersController : ControllerBase
{
    private readonly AppDbContext _context;

    public TeachersController(AppDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    public async Task<IActionResult> GetTeachers()
    {
        var teachers = await _context.Teachers
            .Include(t => t.Lessons)
            .ToListAsync();

        return Ok(teachers);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetTeacher(int id)
    {
        var teacher = await _context.Teachers
            .Include(t => t.Lessons)
            .FirstOrDefaultAsync(t => t.Id == id);

        if (teacher == null)
            return NotFound("Teacher not found");

        return Ok(teacher);
    }

    [HttpPost("{id}/lesson")]
    public async Task<IActionResult> AddLesson(int id, Lesson lesson)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var teacher = await _context.Teachers
            .FirstOrDefaultAsync(t => t.Id == id);

        if (teacher == null)
            return NotFound();

        lesson.TeacherId = id;

        _context.Lessons.Add(lesson);
        await _context.SaveChangesAsync();

        return Ok(lesson);
    }

    [HttpDelete("{teacherId}/lesson/{lessonId}")]
    public async Task<IActionResult> DeleteLesson(int teacherId, int lessonId)
    {
        var lesson = await _context.Lessons
            .FirstOrDefaultAsync(l => l.Id == lessonId && l.TeacherId == teacherId);

        if (lesson == null)
            return NotFound("Lesson not found");

        _context.Lessons.Remove(lesson);
        await _context.SaveChangesAsync();

        return Ok(new { message = "Deleted successfully" });
    }
}