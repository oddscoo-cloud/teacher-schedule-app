namespace backend.Infrastructure.Services;

using backend.Core.Entities;
using backend.Core.Interfaces.Services;
using backend.Data;
using Microsoft.EntityFrameworkCore;

public class CourseService : ICourseService
{
    private readonly AppDbContext _db;
    private readonly StudentWorkspaceAccessService _workspaceAccess;

    public CourseService(AppDbContext db, StudentWorkspaceAccessService workspaceAccess)
    {
        _db = db;
        _workspaceAccess = workspaceAccess;
    }

    public async Task<IReadOnlyList<CourseEntity>?> GetCoursesAsync(int userId, UserRole role, int membershipId)
    {
        var access = await _workspaceAccess.ResolveAsync(userId, role, membershipId);
        if (access == null)
            return null;
        if (!access.SchoolId.HasValue)
            return Array.Empty<CourseEntity>();

        return await _db.Courses
            .AsNoTracking()
            .Where(course => course.SchoolId == access.SchoolId)
            .OrderBy(course => course.Name)
            .ToListAsync();
    }

    public async Task<CourseEntity?> CreateCourseAsync(
        int userId,
        UserRole role,
        int membershipId,
        string name,
        string? description)
    {
        var access = await _workspaceAccess.ResolveAsync(userId, role, membershipId);
        if (role != UserRole.SchoolAdmin || access?.SchoolId == null)
            return null;

        var course = new CourseEntity
        {
            SchoolId = access.SchoolId.Value,
            Name = name.Trim(),
            Description = description?.Trim(),
            CreatedAt = DateTime.UtcNow
        };
        _db.Courses.Add(course);
        await _db.SaveChangesAsync();
        return course;
    }

    public async Task<CourseEntity?> UpdateCourseAsync(
        int userId,
        UserRole role,
        int membershipId,
        int courseId,
        string name,
        string? description)
    {
        var access = await _workspaceAccess.ResolveAsync(userId, role, membershipId);
        if (role != UserRole.SchoolAdmin || access?.SchoolId == null)
            return null;

        var course = await _db.Courses
            .SingleOrDefaultAsync(item => item.Id == courseId && item.SchoolId == access.SchoolId);
        if (course == null)
            return null;

        course.Name = name.Trim();
        course.Description = description?.Trim();
        course.UpdatedAt = DateTime.UtcNow;
        await _db.SaveChangesAsync();
        return course;
    }

    public async Task<bool> DeleteCourseAsync(int userId, UserRole role, int membershipId, int courseId)
    {
        var access = await _workspaceAccess.ResolveAsync(userId, role, membershipId);
        if (role != UserRole.SchoolAdmin || access?.SchoolId == null)
            return false;

        var course = await _db.Courses
            .SingleOrDefaultAsync(item => item.Id == courseId && item.SchoolId == access.SchoolId);
        if (course == null)
            return false;

        _db.Courses.Remove(course);
        await _db.SaveChangesAsync();
        return true;
    }
}
