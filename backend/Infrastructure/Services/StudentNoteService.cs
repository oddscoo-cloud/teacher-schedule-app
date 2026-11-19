namespace backend.Infrastructure.Services;

using backend.Core.Entities;
using backend.Core.Interfaces.Services;
using backend.Data;
using Microsoft.EntityFrameworkCore;

public class StudentNoteService : IStudentNoteService
{
    private readonly AppDbContext _db;
    private readonly StudentWorkspaceAccessService _workspaceAccess;

    public StudentNoteService(AppDbContext db, StudentWorkspaceAccessService workspaceAccess)
    {
        _db = db;
        _workspaceAccess = workspaceAccess;
    }

    public async Task<IReadOnlyList<StudentNoteEntity>?> GetNotesAsync(
        int userId,
        UserRole role,
        int membershipId,
        int studentId)
    {
        var access = await ResolveVisibleStudentAsync(userId, role, membershipId, studentId);
        if (access == null)
            return null;

        var notes = _db.StudentNotes
            .AsNoTracking()
            .Where(note => note.StudentId == studentId);
        if (role != UserRole.SchoolAdmin)
            notes = notes.Where(note => note.TeacherId == access.TeacherId);
        else
            notes = notes.Where(note => note.Teacher.SchoolId == access.SchoolId);

        return await notes.OrderByDescending(note => note.CreatedAt).ToListAsync();
    }

    public async Task<StudentNoteEntity?> CreateNoteAsync(
        int userId,
        UserRole role,
        int membershipId,
        int studentId,
        string content,
        string? goals,
        string? progress)
    {
        var access = await ResolveVisibleStudentAsync(userId, role, membershipId, studentId);
        if (role != UserRole.Teacher || access?.TeacherId == null)
            return null;

        var note = new StudentNoteEntity
        {
            StudentId = studentId,
            TeacherId = access.TeacherId.Value,
            Content = content.Trim(),
            Goals = goals?.Trim(),
            Progress = progress?.Trim(),
            CreatedAt = DateTime.UtcNow
        };
        _db.StudentNotes.Add(note);
        await _db.SaveChangesAsync();
        return note;
    }

    public async Task<StudentNoteEntity?> UpdateNoteAsync(
        int userId,
        UserRole role,
        int membershipId,
        int studentId,
        int noteId,
        string content,
        string? goals,
        string? progress)
    {
        var access = await ResolveVisibleStudentAsync(userId, role, membershipId, studentId);
        if (role != UserRole.Teacher || access?.TeacherId == null)
            return null;

        var note = await _db.StudentNotes.SingleOrDefaultAsync(item =>
            item.Id == noteId &&
            item.StudentId == studentId &&
            item.TeacherId == access.TeacherId);
        if (note == null)
            return null;

        note.Content = content.Trim();
        note.Goals = goals?.Trim();
        note.Progress = progress?.Trim();
        note.UpdatedAt = DateTime.UtcNow;
        await _db.SaveChangesAsync();
        return note;
    }

    public async Task<bool> DeleteNoteAsync(int userId, UserRole role, int membershipId, int studentId, int noteId)
    {
        var access = await ResolveVisibleStudentAsync(userId, role, membershipId, studentId);
        if (role != UserRole.Teacher || access?.TeacherId == null)
            return false;

        var note = await _db.StudentNotes.SingleOrDefaultAsync(item =>
            item.Id == noteId &&
            item.StudentId == studentId &&
            item.TeacherId == access.TeacherId);
        if (note == null)
            return false;

        _db.StudentNotes.Remove(note);
        await _db.SaveChangesAsync();
        return true;
    }

    private async Task<StudentWorkspaceAccess?> ResolveVisibleStudentAsync(
        int userId,
        UserRole role,
        int membershipId,
        int studentId)
    {
        var access = await _workspaceAccess.ResolveAsync(userId, role, membershipId);
        if (access == null ||
            !await _workspaceAccess.VisibleStudents(access).AnyAsync(student => student.Id == studentId))
            return null;

        return access;
    }
}
