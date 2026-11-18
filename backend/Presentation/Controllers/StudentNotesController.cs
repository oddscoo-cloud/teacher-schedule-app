namespace backend.Presentation.Controllers;

using backend.Core.Interfaces.Services;
using backend.Presentation.DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

[ApiController]
[Route("api/students/{studentId:int}/notes")]
[Authorize]
public class StudentNotesController : AuthorizedWorkspaceControllerBase
{
    private readonly IStudentNoteService _notes;

    public StudentNotesController(IStudentNoteService notes)
    {
        _notes = notes;
    }

    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<StudentNoteResponseDto>>> GetNotes(int studentId)
    {
        if (!TryGetRequester(out var requester))
            return Unauthorized();

        var notes = await _notes.GetNotesAsync(
            requester.UserId,
            requester.Role,
            requester.MembershipId,
            studentId);
        if (notes == null)
            return NotFound();

        return Ok(notes.Select(ToResponse));
    }

    [HttpPost]
    public async Task<ActionResult<StudentNoteResponseDto>> CreateNote(int studentId, StudentNoteRequestDto request)
    {
        if (!TryGetRequester(out var requester))
            return Unauthorized();

        var note = await _notes.CreateNoteAsync(
            requester.UserId,
            requester.Role,
            requester.MembershipId,
            studentId,
            request.Content,
            request.Goals,
            request.Progress);
        return note == null ? NotFound() : CreatedAtAction(nameof(GetNotes), new { studentId }, ToResponse(note));
    }

    [HttpPut("{noteId:int}")]
    public async Task<ActionResult<StudentNoteResponseDto>> UpdateNote(
        int studentId,
        int noteId,
        StudentNoteRequestDto request)
    {
        if (!TryGetRequester(out var requester))
            return Unauthorized();

        var note = await _notes.UpdateNoteAsync(
            requester.UserId,
            requester.Role,
            requester.MembershipId,
            studentId,
            noteId,
            request.Content,
            request.Goals,
            request.Progress);
        return note == null ? NotFound() : Ok(ToResponse(note));
    }

    [HttpDelete("{noteId:int}")]
    public async Task<IActionResult> DeleteNote(int studentId, int noteId)
    {
        if (!TryGetRequester(out var requester))
            return Unauthorized();

        var deleted = await _notes.DeleteNoteAsync(
            requester.UserId,
            requester.Role,
            requester.MembershipId,
            studentId,
            noteId);
        return deleted ? NoContent() : NotFound();
    }

    private static StudentNoteResponseDto ToResponse(backend.Core.Entities.StudentNoteEntity note) => new()
    {
        Id = note.Id,
        TeacherId = note.TeacherId,
        Content = note.Content,
        Goals = note.Goals,
        Progress = note.Progress,
        CreatedAt = note.CreatedAt,
        UpdatedAt = note.UpdatedAt
    };
}
