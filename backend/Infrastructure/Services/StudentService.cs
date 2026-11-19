namespace backend.Infrastructure.Services;

using backend.Core.Entities;
using backend.Core.Interfaces.Services;
using backend.Data;
using Microsoft.EntityFrameworkCore;

public class StudentService : IStudentService
{
    private readonly AppDbContext _db;
    private readonly StudentWorkspaceAccessService _workspaceAccess;

    public StudentService(AppDbContext db, StudentWorkspaceAccessService workspaceAccess)
    {
        _db = db;
        _workspaceAccess = workspaceAccess;
    }

    public async Task<IReadOnlyList<StudentEntity>?> GetStudentsAsync(
        int userId,
        UserRole role,
        int membershipId,
        string? search,
        int? courseId,
        AccountStatus? status)
    {
        var access = await _workspaceAccess.ResolveAsync(userId, role, membershipId);
        if (access == null)
            return null;

        var query = _workspaceAccess.VisibleStudents(access);
        if (!string.IsNullOrWhiteSpace(search))
        {
            var term = search.Trim();
            query = query.Where(student =>
                student.FirstName.Contains(term) ||
                student.LastName.Contains(term) ||
                (student.Patronymic != null && student.Patronymic.Contains(term)) ||
                (student.Email != null && student.Email.Contains(term)));
        }

        if (courseId.HasValue)
            query = query.Where(student => student.CourseAssignments.Any(course => course.CourseId == courseId.Value));
        if (status.HasValue)
            query = query.Where(student => student.Status == status.Value);

        return await query
            .AsNoTracking()
            .OrderBy(student => student.LastName)
            .ThenBy(student => student.FirstName)
            .ToListAsync();
    }

    public async Task<StudentEntity?> GetStudentAsync(int userId, UserRole role, int membershipId, int studentId)
    {
        var access = await _workspaceAccess.ResolveAsync(userId, role, membershipId);
        if (access == null)
            return null;

        IQueryable<StudentEntity> query = _workspaceAccess.VisibleStudents(access)
            .Where(student => student.Id == studentId)
            .Include(student => student.User)
            .Include(student => student.Contacts)
                .ThenInclude(link => link.Contact)
            .Include(student => student.CourseAssignments.Where(link =>
                access.SchoolId.HasValue && link.Course.SchoolId == access.SchoolId))
                .ThenInclude(link => link.Course)
            .Include(student => student.TeacherAssignments.Where(assignment =>
                assignment.IsSchool == (access.Type == WorkspaceType.School) &&
                (access.Role == UserRole.SchoolAdmin || assignment.TeacherId == access.TeacherId)))
            .ThenInclude(assignment => assignment.Teacher)
            .ThenInclude(teacher => teacher.User);

        if (role == UserRole.Teacher && access.TeacherId.HasValue)
            query = query.Include(student => student.Notes.Where(note => note.TeacherId == access.TeacherId.Value));
        else
            query = query.Include(student => student.Notes.Where(note => note.Teacher.SchoolId == access.SchoolId));

        return await query.AsNoTracking().SingleOrDefaultAsync();
    }

    public async Task<StudentEntity?> CreateStudentAsync(
        int userId,
        UserRole role,
        int membershipId,
        StudentEntity student,
        IReadOnlyCollection<int> courseIds,
        IReadOnlyCollection<StudentContactEntity> contacts)
    {
        var access = await _workspaceAccess.ResolveAsync(userId, role, membershipId);
        if (access == null)
            return null;

        if (role == UserRole.SchoolAdmin && access.SchoolId.HasValue)
        {
            student.SchoolId = access.SchoolId;
            student.StandaloneTeacherId = null;
        }
        else if (role == UserRole.Teacher &&
                 access.Type == WorkspaceType.Standalone &&
                 access.TeacherId.HasValue)
        {
            student.SchoolId = null;
            student.StandaloneTeacherId = access.TeacherId;
            student.TeacherAssignments.Add(new StudentAssignmentEntity
            {
                TeacherId = access.TeacherId.Value,
                IsSchool = false,
                AssignedAt = DateTime.UtcNow
            });
        }
        else
        {
            return null;
        }

        var courses = await GetCoursesAsync(access, courseIds);
        if (courses == null)
            return null;

        student.UserId = null;
        student.Status = AccountStatus.Active;
        student.CreatedAt = DateTime.UtcNow;
        foreach (var course in courses)
        {
            student.CourseAssignments.Add(new StudentCourseAssignmentEntity { CourseId = course.Id });
        }
        foreach (var contact in contacts)
            student.Contacts.Add(contact);

        _db.StudentEntities.Add(student);
        await _db.SaveChangesAsync();
        return student;
    }

    public async Task<StudentEntity?> UpdateStudentAsync(
        int userId,
        UserRole role,
        int membershipId,
        int studentId,
        StudentEntity updatedStudent,
        IReadOnlyCollection<int> courseIds,
        IReadOnlyCollection<StudentContactEntity> contacts)
    {
        var access = await _workspaceAccess.ResolveAsync(userId, role, membershipId);
        if (access == null)
            return null;

        var student = await _workspaceAccess.VisibleStudents(access)
            .Include(item => item.CourseAssignments)
            .Include(item => item.Contacts)
            .SingleOrDefaultAsync(item => item.Id == studentId);
        if (student == null)
            return null;

        if (role != UserRole.SchoolAdmin && role != UserRole.Teacher)
            return null;

        var courses = await GetCoursesAsync(access, courseIds);
        if (courses == null)
            return null;

        student.FirstName = updatedStudent.FirstName;
        student.LastName = updatedStudent.LastName;
        student.Patronymic = updatedStudent.Patronymic;
        student.Email = updatedStudent.Email;
        student.PhoneNumber = updatedStudent.PhoneNumber;
        student.DateOfBirth = updatedStudent.DateOfBirth;
        student.UpdatedAt = DateTime.UtcNow;

        student.CourseAssignments.Clear();
        _db.StudentContacts.RemoveRange(student.Contacts);
        student.Contacts.Clear();
        foreach (var course in courses)
            student.CourseAssignments.Add(new StudentCourseAssignmentEntity { StudentId = studentId, CourseId = course.Id });
        foreach (var contact in contacts)
            student.Contacts.Add(contact);

        await _db.SaveChangesAsync();
        return student;
    }

    public async Task<bool> DeleteStudentAsync(
        int userId,
        UserRole role,
        int membershipId,
        int studentId)
    {
        var access = await _workspaceAccess.ResolveAsync(userId, role, membershipId);
        if (access == null)
            return false;

        var student = await _workspaceAccess.VisibleStudents(access)
            .Include(item => item.Contacts)
            .Include(item => item.CourseAssignments)
            .Include(item => item.TeacherAssignments)
            .Include(item => item.Notes)
            .SingleOrDefaultAsync(item => item.Id == studentId);
        if (student == null)
            return false;

        _db.StudentContacts.RemoveRange(student.Contacts);
        _db.StudentNotes.RemoveRange(student.Notes);
        _db.Set<StudentCourseAssignmentEntity>().RemoveRange(student.CourseAssignments);
        _db.StudentAssignments.RemoveRange(student.TeacherAssignments);
        _db.StudentEntities.Remove(student);
        await _db.SaveChangesAsync();
        return true;
    }

    private async Task<List<CourseEntity>?> GetCoursesAsync(
        StudentWorkspaceAccess access,
        IReadOnlyCollection<int> courseIds)
    {
        if (courseIds.Count == 0)
            return new List<CourseEntity>();
        if (!access.SchoolId.HasValue)
            return null;

        var distinctIds = courseIds.Distinct().ToArray();
        var courses = await _db.Courses
            .Where(course => course.SchoolId == access.SchoolId && distinctIds.Contains(course.Id))
            .ToListAsync();
        return courses.Count == distinctIds.Length ? courses : null;
    }
}
