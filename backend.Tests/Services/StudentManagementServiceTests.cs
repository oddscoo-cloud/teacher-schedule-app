using backend.Core.Entities;
using backend.Core.Interfaces.Services;
using backend.Data;
using backend.Infrastructure.Services;
using Microsoft.EntityFrameworkCore;

namespace backend.Tests.Services;

public class StudentManagementServiceTests
{
    [Fact]
    public async Task GetStudentsAsync_TeacherSeesOnlyAssignedStudentsInCurrentSchool()
    {
        await using var db = CreateContext();
        await SeedWorkspaceDataAsync(db);
        var access = new StudentWorkspaceAccessService(db);
        var service = new StudentService(db, access);

        var students = await service.GetStudentsAsync(
            userId: 2,
            UserRole.Teacher,
            membershipId: 101,
            search: null,
            courseId: null,
            status: null);

        Assert.NotNull(students);
        Assert.Equal(new[] { 1 }, students.Select(student => student.Id));
    }

    [Fact]
    public async Task GetStudentsAsync_AdminSeesAllAndOnlyStudentsInTheirSchool()
    {
        await using var db = CreateContext();
        await SeedWorkspaceDataAsync(db);
        var access = new StudentWorkspaceAccessService(db);
        var service = new StudentService(db, access);

        var students = await service.GetStudentsAsync(
            userId: 1,
            UserRole.SchoolAdmin,
            membershipId: 100,
            search: null,
            courseId: null,
            status: null);

        Assert.NotNull(students);
        Assert.Equal(new[] { 1, 2 }, students.Select(student => student.Id));
    }

    [Fact]
    public async Task GetStudentsAsync_StandaloneTeacherDoesNotSeeSchoolStudents()
    {
        await using var db = CreateContext();
        await SeedWorkspaceDataAsync(db);
        var access = new StudentWorkspaceAccessService(db);
        var service = new StudentService(db, access);

        var students = await service.GetStudentsAsync(
            userId: 4,
            UserRole.Teacher,
            membershipId: 103,
            search: null,
            courseId: null,
            status: null);

        Assert.NotNull(students);
        Assert.Equal(new[] { 4 }, students.Select(student => student.Id));
    }

    [Fact]
    public async Task GetNotesAsync_TeacherSeesOnlyTheirNotesAndAdminSeesSchoolNotes()
    {
        await using var db = CreateContext();
        await SeedWorkspaceDataAsync(db);
        var access = new StudentWorkspaceAccessService(db);
        var service = new StudentNoteService(db, access);

        var teacherNotes = await service.GetNotesAsync(2, UserRole.Teacher, 101, studentId: 1);
        var adminNotes = await service.GetNotesAsync(1, UserRole.SchoolAdmin, 100, studentId: 1);

        Assert.NotNull(teacherNotes);
        Assert.Equal(new[] { 1 }, teacherNotes.Select(note => note.Id));
        Assert.NotNull(adminNotes);
        Assert.Equal(new[] { 1, 2 }, adminNotes.Select(note => note.Id));
    }

    [Fact]
    public async Task CreateStudentAsync_RejectsTeacherInSchoolWorkspace()
    {
        await using var db = CreateContext();
        await SeedWorkspaceDataAsync(db);
        var access = new StudentWorkspaceAccessService(db);
        var service = new StudentService(db, access);

        var student = await service.CreateStudentAsync(
            userId: 2,
            UserRole.Teacher,
            membershipId: 101,
            new StudentEntity { FirstName = "New", LastName = "Student" },
            Array.Empty<int>(),
            Array.Empty<StudentContactEntity>());

        Assert.Null(student);
    }

    private static AppDbContext CreateContext()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        return new AppDbContext(options);
    }

    private static async Task SeedWorkspaceDataAsync(AppDbContext db)
    {
        db.Schools.AddRange(
            new SchoolEntity { Id = 1, Name = "School One" },
            new SchoolEntity { Id = 2, Name = "School Two" });
        db.UserEntities.AddRange(
            new UserEntity { Id = 1, Email = "admin@example.test", PasswordHash = "hash", Role = UserRole.SchoolAdmin, Status = AccountStatus.Active },
            new UserEntity { Id = 2, Email = "teacher@example.test", PasswordHash = "hash", Role = UserRole.Teacher, Status = AccountStatus.Active },
            new UserEntity { Id = 3, Email = "other@example.test", PasswordHash = "hash", Role = UserRole.Teacher, Status = AccountStatus.Active },
            new UserEntity { Id = 4, Email = "solo@example.test", PasswordHash = "hash", Role = UserRole.Teacher, Status = AccountStatus.Active });
        db.TeacherEntities.AddRange(
            new TeacherEntity { Id = 10, UserId = 2, SchoolId = 1, FirstName = "A", LastName = "Teacher" },
            new TeacherEntity { Id = 11, UserId = 3, SchoolId = 1, FirstName = "B", LastName = "Teacher" },
            new TeacherEntity { Id = 12, UserId = 4, FirstName = "Solo", LastName = "Teacher" });
        db.WorkspaceMemberships.AddRange(
            new WorkspaceMembershipEntity { Id = 100, UserId = 1, SchoolId = 1, WorkspaceType = WorkspaceType.School },
            new WorkspaceMembershipEntity { Id = 101, UserId = 2, SchoolId = 1, WorkspaceType = WorkspaceType.School },
            new WorkspaceMembershipEntity { Id = 102, UserId = 3, SchoolId = 1, WorkspaceType = WorkspaceType.School },
            new WorkspaceMembershipEntity { Id = 103, UserId = 4, WorkspaceType = WorkspaceType.Standalone });
        db.StudentEntities.AddRange(
            new StudentEntity { Id = 1, SchoolId = 1, FirstName = "One", LastName = "Student", Status = AccountStatus.Active },
            new StudentEntity { Id = 2, SchoolId = 1, FirstName = "Two", LastName = "Student", Status = AccountStatus.Active },
            new StudentEntity { Id = 3, SchoolId = 2, FirstName = "Three", LastName = "Student", Status = AccountStatus.Active },
            new StudentEntity { Id = 4, StandaloneTeacherId = 12, FirstName = "Four", LastName = "Student", Status = AccountStatus.Active });
        db.StudentAssignments.AddRange(
            new StudentAssignmentEntity { Id = 1, StudentId = 1, TeacherId = 10, IsSchool = true },
            new StudentAssignmentEntity { Id = 2, StudentId = 1, TeacherId = 11, IsSchool = true },
            new StudentAssignmentEntity { Id = 3, StudentId = 3, TeacherId = 10, IsSchool = true },
            new StudentAssignmentEntity { Id = 4, StudentId = 4, TeacherId = 12, IsSchool = false });
        db.StudentNotes.AddRange(
            new StudentNoteEntity { Id = 1, StudentId = 1, TeacherId = 10, Content = "Private one" },
            new StudentNoteEntity { Id = 2, StudentId = 1, TeacherId = 11, Content = "Private two" });
        await db.SaveChangesAsync();
    }
}
