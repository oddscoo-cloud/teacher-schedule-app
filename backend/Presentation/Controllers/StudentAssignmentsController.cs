namespace backend.Presentation.Controllers;

using backend.Core.Interfaces.Services;
using backend.Presentation.DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

[ApiController]
[Authorize]
public class StudentAssignmentsController : AuthorizedWorkspaceControllerBase
{
    private readonly IStudentAssignmentService _assignments;

    public StudentAssignmentsController(IStudentAssignmentService assignments)
    {
        _assignments = assignments;
    }

    [HttpPost("api/students/{studentId:int}/assign-teacher/{teacherId:int}")]
    public async Task<IActionResult> AssignTeacher(int studentId, int teacherId)
    {
        if (!TryGetRequester(out var requester))
            return Unauthorized();

        var assigned = await _assignments.AssignTeacherAsync(
            requester.UserId,
            requester.Role,
            requester.MembershipId,
            studentId,
            teacherId);
        return assigned ? NoContent() : Forbid();
    }

    [HttpDelete("api/students/{studentId:int}/assign-teacher/{teacherId:int}")]
    public async Task<IActionResult> RevokeTeacher(int studentId, int teacherId)
    {
        if (!TryGetRequester(out var requester))
            return Unauthorized();

        var revoked = await _assignments.RevokeTeacherAsync(
            requester.UserId,
            requester.Role,
            requester.MembershipId,
            studentId,
            teacherId);
        return revoked ? NoContent() : NotFound();
    }

    [HttpGet("~/api/students/{studentId:int}/assignments")]
    public async Task<ActionResult<IReadOnlyList<StudentAssignmentResponseDto>>> GetAssignments(int studentId)
    {
        if (!TryGetRequester(out var requester))
            return Unauthorized();

        var assignments = await _assignments.GetAssignmentsAsync(
            requester.UserId,
            requester.Role,
            requester.MembershipId,
            studentId);
        if (assignments == null)
            return NotFound();

        return Ok(assignments.Select(assignment => new StudentAssignmentResponseDto
        {
            TeacherId = assignment.TeacherId,
            TeacherName = assignment.Teacher.FirstName + " " + assignment.Teacher.LastName,
            TeacherEmail = assignment.Teacher.User.Email,
            AssignedAt = assignment.AssignedAt
        }));
    }
}
