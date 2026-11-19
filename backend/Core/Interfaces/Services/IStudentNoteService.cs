namespace backend.Core.Interfaces.Services;

using backend.Core.Entities;

public interface IStudentNoteService
{
    Task<IReadOnlyList<StudentNoteEntity>?> GetNotesAsync(int userId, UserRole role, int membershipId, int studentId);
    Task<StudentNoteEntity?> CreateNoteAsync(int userId, UserRole role, int membershipId, int studentId, string content, string? goals, string? progress);
    Task<StudentNoteEntity?> UpdateNoteAsync(int userId, UserRole role, int membershipId, int studentId, int noteId, string content, string? goals, string? progress);
    Task<bool> DeleteNoteAsync(int userId, UserRole role, int membershipId, int studentId, int noteId);
}
