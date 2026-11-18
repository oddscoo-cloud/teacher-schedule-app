namespace backend.Infrastructure.Services;

using backend.Core.Entities;
using backend.Data;
using Microsoft.EntityFrameworkCore;

public sealed record StudentWorkspaceAccess(
    UserRole Role,
    WorkspaceType Type,
    int? SchoolId,
    int? TeacherId);

public sealed class StudentWorkspaceAccessService
{
    private readonly AppDbContext _db;

    public StudentWorkspaceAccessService(AppDbContext db)
    {
        _db = db;
    }

    public async Task<StudentWorkspaceAccess?> ResolveAsync(int userId, UserRole role, int membershipId)
    {
        var membership = await _db.WorkspaceMemberships
            .AsNoTracking()
            .Where(item => item.Id == membershipId && item.UserId == userId)
            .Select(item => new { item.WorkspaceType, item.SchoolId })
            .SingleOrDefaultAsync();

        if (membership == null)
            return null;

        if (role == UserRole.SchoolAdmin)
        {
            return membership.WorkspaceType == WorkspaceType.School && membership.SchoolId.HasValue
                ? new StudentWorkspaceAccess(role, membership.WorkspaceType, membership.SchoolId, null)
                : null;
        }

        if (role != UserRole.Teacher)
            return null;

        var teacherQuery = _db.TeacherEntities.AsNoTracking().Where(teacher => teacher.UserId == userId);
        if (membership.WorkspaceType == WorkspaceType.School)
            teacherQuery = teacherQuery.Where(teacher => teacher.SchoolId == membership.SchoolId);
        else
            teacherQuery = teacherQuery.Where(teacher => teacher.SchoolId == null);

        var teacherId = await teacherQuery
            .Select(teacher => (int?)teacher.Id)
            .SingleOrDefaultAsync();

        if (!teacherId.HasValue)
            return null;

        if (membership.WorkspaceType == WorkspaceType.School && membership.SchoolId.HasValue)
            return new StudentWorkspaceAccess(role, membership.WorkspaceType, membership.SchoolId, teacherId);

        if (membership.WorkspaceType == WorkspaceType.Standalone && !membership.SchoolId.HasValue)
            return new StudentWorkspaceAccess(role, membership.WorkspaceType, null, teacherId);

        return null;
    }

    public IQueryable<StudentEntity> VisibleStudents(StudentWorkspaceAccess access)
    {
        if (access.Role == UserRole.SchoolAdmin && access.SchoolId.HasValue)
            return _db.StudentEntities.Where(student => student.SchoolId == access.SchoolId);

        if (access.Role != UserRole.Teacher || !access.TeacherId.HasValue)
            return _db.StudentEntities.Where(_ => false);

        if (access.Type == WorkspaceType.School && access.SchoolId.HasValue)
        {
            return _db.StudentEntities.Where(student =>
                student.SchoolId == access.SchoolId &&
                student.TeacherAssignments.Any(assignment =>
                    assignment.TeacherId == access.TeacherId && assignment.IsSchool));
        }

        return _db.StudentEntities.Where(student =>
            student.SchoolId == null &&
            student.StandaloneTeacherId == access.TeacherId);
    }
}
