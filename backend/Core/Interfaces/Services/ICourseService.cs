namespace backend.Core.Interfaces.Services;

using backend.Core.Entities;

public interface ICourseService
{
    Task<IReadOnlyList<CourseEntity>?> GetCoursesAsync(int userId, UserRole role, int membershipId);
    Task<CourseEntity?> CreateCourseAsync(int userId, UserRole role, int membershipId, string name, string? description);
    Task<CourseEntity?> UpdateCourseAsync(int userId, UserRole role, int membershipId, int courseId, string name, string? description);
    Task<bool> DeleteCourseAsync(int userId, UserRole role, int membershipId, int courseId);
}
