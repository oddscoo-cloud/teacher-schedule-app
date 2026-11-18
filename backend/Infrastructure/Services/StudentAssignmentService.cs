namespace backend.Infrastructure.Services;

using backend.Core.Entities;
using backend.Core.Interfaces.Services;
using backend.Data;
using Microsoft.EntityFrameworkCore;

public class StudentAssignmentService : IStudentAssignmentService
{
    private readonly AppDbContext _db;
    private readonly StudentWorkspaceAccessService _workspaceAccess;

    public StudentAssignmentService(AppDbContext db, StudentWorkspaceAccessService workspaceAccess)
    {
        _db = db;
        _workspaceAccess = workspaceAccess;
    }

    public async Task<bool> AssignTeacherAsync(
        int userId,
        UserRole role,
        int membershipId,
        int studentId,
        int teacherId)
    {
        var access = await _workspaceAccess.ResolveAsync(userId, role, membershipId);
        if (role != UserRole.SchoolAdmin || access?.SchoolId == null)
            return false;

        var student = await _db.StudentEntities.SingleOrDefaultAsync(item =>
            item.Id == studentId && item.SchoolId == access.SchoolId);
        var teacher = await _db.TeacherEntities.SingleOrDefaultAsync(item => item.Id == teacherId);
        if (student == null || teacher == null || teacher.SchoolId != access.SchoolId ||
            !await _db.WorkspaceMemberships.AnyAsync(item =>
                item.UserId == teacher.UserId &&
                item.WorkspaceType == WorkspaceType.School &&
                item.SchoolId == access.SchoolId))
            return false;

        var assignment = await _db.StudentAssignments
            .SingleOrDefaultAsync(item => item.StudentId == studentId && item.TeacherId == teacherId);
        if (assignment == null)
        {
            _db.StudentAssignments.Add(new StudentAssignmentEntity
            {
                StudentId = studentId,
                TeacherId = teacherId,
                AssignedAt = DateTime.UtcNow,
                IsSchool = true
            });
        }
        else
        {
            assignment.IsSchool = true;
        }

        await _db.SaveChangesAsync();
        return true;
    }

    public async Task<bool> RevokeTeacherAsync(
        int userId,
        UserRole role,
        int membershipId,
        int studentId,
        int teacherId)
    {
        var access = await _workspaceAccess.ResolveAsync(userId, role, membershipId);
        if (role != UserRole.SchoolAdmin || access?.SchoolId == null)
            return false;

        var assignment = await _db.StudentAssignments
            .SingleOrDefaultAsync(item =>
                item.StudentId == studentId &&
                item.TeacherId == teacherId &&
                item.IsSchool &&
                item.Student.SchoolId == access.SchoolId);
        if (assignment == null)
            return false;

        _db.StudentAssignments.Remove(assignment);
        await _db.SaveChangesAsync();
        return true;
    }

    public async Task<IReadOnlyList<StudentAssignmentEntity>?> GetAssignmentsAsync(
        int userId,
        UserRole role,
        int membershipId,
        int studentId)
    {
        var access = await _workspaceAccess.ResolveAsync(userId, role, membershipId);
        if (access == null || !await _workspaceAccess.VisibleStudents(access).AnyAsync(item => item.Id == studentId))
            return null;

        return await _db.StudentAssignments
            .AsNoTracking()
            .Where(item =>
                item.StudentId == studentId &&
                item.IsSchool == (access.Type == WorkspaceType.School) &&
                (access.Role == UserRole.SchoolAdmin || item.TeacherId == access.TeacherId))
            .Include(item => item.Teacher)
                .ThenInclude(teacher => teacher.User)
            .ToListAsync();
    }
}
