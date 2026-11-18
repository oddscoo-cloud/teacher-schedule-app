namespace backend.Presentation.Controllers;

using backend.Core.Interfaces.Services;
using backend.Presentation.DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

[ApiController]
[Route("api/courses")]
[Authorize]
public class CoursesController : AuthorizedWorkspaceControllerBase
{
    private readonly ICourseService _courses;

    public CoursesController(ICourseService courses)
    {
        _courses = courses;
    }

    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<CourseResponseDto>>> GetCourses()
    {
        if (!TryGetRequester(out var requester))
            return Unauthorized();

        var courses = await _courses.GetCoursesAsync(requester.UserId, requester.Role, requester.MembershipId);
        if (courses == null)
            return Forbid();

        return Ok(courses.Select(ToResponse));
    }

    [HttpPost]
    public async Task<ActionResult<CourseResponseDto>> CreateCourse(CourseRequestDto request)
    {
        if (!TryGetRequester(out var requester))
            return Unauthorized();

        var course = await _courses.CreateCourseAsync(
            requester.UserId,
            requester.Role,
            requester.MembershipId,
            request.Name,
            request.Description);
        return course == null ? Forbid() : CreatedAtAction(nameof(GetCourses), ToResponse(course));
    }

    [HttpPut("{id:int}")]
    public async Task<ActionResult<CourseResponseDto>> UpdateCourse(int id, CourseRequestDto request)
    {
        if (!TryGetRequester(out var requester))
            return Unauthorized();

        var course = await _courses.UpdateCourseAsync(
            requester.UserId,
            requester.Role,
            requester.MembershipId,
            id,
            request.Name,
            request.Description);
        return course == null ? NotFound() : Ok(ToResponse(course));
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> DeleteCourse(int id)
    {
        if (!TryGetRequester(out var requester))
            return Unauthorized();

        var deleted = await _courses.DeleteCourseAsync(requester.UserId, requester.Role, requester.MembershipId, id);
        return deleted ? NoContent() : NotFound();
    }

    private static CourseResponseDto ToResponse(backend.Core.Entities.CourseEntity course) => new()
    {
        Id = course.Id,
        Name = course.Name,
        Description = course.Description
    };
}
