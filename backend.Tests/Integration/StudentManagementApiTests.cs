using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Security.Claims;
using System.Text.Json;
using backend.Core.Entities;
using backend.Data;
using backend.Infrastructure.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Text.Encodings.Web;

namespace backend.Tests.Integration;

public class StudentManagementApiTests : IClassFixture<StudentManagementApiFactory>, IAsyncLifetime
{
    private readonly StudentManagementApiFactory _factory;

    public StudentManagementApiTests(StudentManagementApiFactory factory)
    {
        _factory = factory;
    }

    public Task InitializeAsync() => _factory.SeedAsync();

    public Task DisposeAsync() => Task.CompletedTask;

    [Fact]
    public async Task GetStudents_TeacherReceivesOnlyAssignedSchoolStudents()
    {
        using var client = CreateClient(userId: 2, role: UserRole.Teacher, membershipId: 101);

        var response = await client.GetAsync("/api/students");
        var body = await response.Content.ReadAsStringAsync();

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        using var json = JsonDocument.Parse(body);
        Assert.Equal(new[] { 1, 4 }, json.RootElement.EnumerateArray()
            .Select(student => student.GetProperty("id").GetInt32())
            .OrderBy(id => id));
    }

    [Fact]
    public async Task GetStudents_AdminReceivesAllStudentsInTheirSchoolOnly()
    {
        using var client = CreateClient(userId: 1, role: UserRole.SchoolAdmin, membershipId: 100);

        var response = await client.GetAsync("/api/students");
        var body = await response.Content.ReadAsStringAsync();

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        using var json = JsonDocument.Parse(body);
        Assert.Equal(new[] { 1, 2, 4 }, json.RootElement.EnumerateArray()
            .Select(student => student.GetProperty("id").GetInt32())
            .OrderBy(id => id));
    }

    [Fact]
    public async Task GetNotes_TeacherReceivesOnlyTheirOwnNotes()
    {
        using var client = CreateClient(userId: 2, role: UserRole.Teacher, membershipId: 101);

        var response = await client.GetAsync("/api/students/1/notes");
        var body = await response.Content.ReadAsStringAsync();

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        using var json = JsonDocument.Parse(body);
        Assert.Equal(new[] { 1 }, json.RootElement.EnumerateArray()
            .Select(note => note.GetProperty("id").GetInt32()));
    }

    [Fact]
    public async Task GetStudents_RequiresAuthentication()
    {
        using var client = _factory.CreateClient();

        var response = await client.GetAsync("/api/students");

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task SwitchWorkspace_MembershipBelongingToAnotherUserIsForbidden()
    {
        using var client = CreateClient(userId: 2, role: UserRole.Teacher, membershipId: 101);

        var response = await client.PostAsync("/api/auth/switch-workspace/100", null);

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task GetHomework_TeacherAndStudentAreScopedToTheirAssignments()
    {
        using var teacher = CreateClient(userId: 2, role: UserRole.Teacher, membershipId: 101);
        using var student = CreateClient(userId: 4, role: UserRole.Student, membershipId: 0);

        var teacherResponse = await teacher.GetAsync("/api/homework");
        var studentResponse = await student.GetAsync("/api/homework");
        var inaccessibleHomework = await student.GetAsync("/api/homework/41");

        Assert.Equal(HttpStatusCode.OK, teacherResponse.StatusCode);
        Assert.Equal(HttpStatusCode.OK, studentResponse.StatusCode);
        Assert.Equal(new[] { 41, 44 }, ReadIds(await teacherResponse.Content.ReadAsStringAsync()));
        Assert.Equal(new[] { 44 }, ReadIds(await studentResponse.Content.ReadAsStringAsync()));
        Assert.Equal(HttpStatusCode.NotFound, inaccessibleHomework.StatusCode);
    }

    [Fact]
    public async Task SubmissionGradingAndAttachmentDownloadRequireAuthorizedWorkspace()
    {
        using var student = CreateClient(userId: 4, role: UserRole.Student, membershipId: 0);
        using var teacher = CreateClient(userId: 2, role: UserRole.Teacher, membershipId: 101);
        using var otherTeacher = CreateClient(userId: 3, role: UserRole.Teacher, membershipId: 102);

        using var form = new MultipartFormDataContent();
        form.Add(new StringContent("My completed work"), "Text");
        form.Add(new StringContent("[\"https://example.test/work\"]"), "LinksJson");
        var file = new ByteArrayContent(new byte[] { 0xff, 0xd8, 0xff, 0x00, 0xff, 0xd9 });
        file.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("image/jpeg");
        form.Add(file, "Files", "answer.jpg");

        var submitResponse = await student.PostAsync("/api/homework/44/submissions", form);
        var submission = JsonDocument.Parse(await submitResponse.Content.ReadAsStringAsync());
        var submissionId = submission.RootElement.GetProperty("id").GetInt32();
        var attachmentId = submission.RootElement.GetProperty("attachments")[0].GetProperty("id").GetInt32();

        var deniedDownload = await otherTeacher.GetAsync($"/api/homework/attachments/{attachmentId}");
        var allowedDownload = await student.GetAsync($"/api/homework/attachments/{attachmentId}");
        var deniedGrade = await otherTeacher.PostAsync(
            $"/api/homework/submissions/{submissionId}/grade",
            JsonContent.Create(new { grade = 0 }));
        var gradeResponse = await teacher.PostAsync(
            $"/api/homework/submissions/{submissionId}/grade",
            JsonContent.Create(new { grade = 0 }));
        var studentHomework = await student.GetAsync("/api/homework/44");

        Assert.Equal(HttpStatusCode.OK, submitResponse.StatusCode);
        Assert.Equal(HttpStatusCode.NotFound, deniedDownload.StatusCode);
        Assert.Equal(HttpStatusCode.OK, allowedDownload.StatusCode);
        Assert.Equal("image/jpeg", allowedDownload.Content.Headers.ContentType?.MediaType);
        Assert.Equal(new byte[] { 0xff, 0xd8, 0xff, 0x00, 0xff, 0xd9 },
            await allowedDownload.Content.ReadAsByteArrayAsync());
        Assert.Equal(HttpStatusCode.NotFound, deniedGrade.StatusCode);
        Assert.Equal(HttpStatusCode.NoContent, gradeResponse.StatusCode);
        using var homeworkJson = JsonDocument.Parse(await studentHomework.Content.ReadAsStringAsync());
        Assert.Equal(0, homeworkJson.RootElement.GetProperty("submissions")[0].GetProperty("grade").GetInt32());
    }

    [Fact]
    public async Task GetHomeworks_FiltersByStudentAndCourse()
    {
        using var teacher = CreateClient(userId: 2, role: UserRole.Teacher, membershipId: 101);

        var response = await teacher.GetAsync("/api/homework?studentId=4&courseId=1");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal(new[] { 44 }, ReadIds(await response.Content.ReadAsStringAsync()));
    }

    private static int[] ReadIds(string response)
    {
        using var json = JsonDocument.Parse(response);
        return json.RootElement.EnumerateArray()
            .Select(homework => homework.GetProperty("id").GetInt32())
            .Order()
            .ToArray();
    }

    private HttpClient CreateClient(int userId, UserRole role, int membershipId)
    {
        var client = _factory.CreateClient();
        client.DefaultRequestHeaders.Add(TestAuthenticationHandler.UserIdHeader, userId.ToString());
        client.DefaultRequestHeaders.Add(TestAuthenticationHandler.RoleHeader, role.ToString());
        client.DefaultRequestHeaders.Add(TestAuthenticationHandler.MembershipHeader, membershipId.ToString());
        return client;
    }
}

public class StudentManagementApiFactory : WebApplicationFactory<Program>
{
    private readonly string _databaseName = $"StudentApiTests-{Guid.NewGuid()}";
    private bool _seeded;

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureServices(services =>
        {
            services.RemoveAll<AppDbContext>();
            services.RemoveAll<DbContextOptions<AppDbContext>>();
            services.RemoveAll<IDbContextOptionsConfiguration<AppDbContext>>();
            services.AddDbContext<AppDbContext>(options => options.UseInMemoryDatabase(_databaseName));
            services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = TestAuthenticationHandler.SchemeName;
                options.DefaultChallengeScheme = TestAuthenticationHandler.SchemeName;
            }).AddScheme<AuthenticationSchemeOptions, TestAuthenticationHandler>(
                TestAuthenticationHandler.SchemeName,
                _ => { });
            services.RemoveAll<IHomeworkFileStorage>();
            services.AddSingleton<IHomeworkFileStorage, TestHomeworkFileStorage>();
        });
    }

    public async Task SeedAsync()
    {
        if (_seeded)
            return;

        using var scope = Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        await db.Database.EnsureCreatedAsync();

        db.Schools.Add(new SchoolEntity { Id = 1, Name = "School One" });
        db.UserEntities.AddRange(
            new UserEntity { Id = 1, Email = "admin@example.test", PasswordHash = "hash", Role = UserRole.SchoolAdmin, Status = AccountStatus.Active },
            new UserEntity { Id = 2, Email = "teacher@example.test", PasswordHash = "hash", Role = UserRole.Teacher, Status = AccountStatus.Active },
            new UserEntity { Id = 3, Email = "other@example.test", PasswordHash = "hash", Role = UserRole.Teacher, Status = AccountStatus.Active },
            new UserEntity { Id = 4, Email = "student@example.test", PasswordHash = "hash", Role = UserRole.Student, Status = AccountStatus.Active });
        db.TeacherEntities.AddRange(
            new TeacherEntity { Id = 10, UserId = 2, SchoolId = 1, FirstName = "A", LastName = "Teacher" },
            new TeacherEntity { Id = 11, UserId = 3, SchoolId = 1, FirstName = "B", LastName = "Teacher" });
        db.WorkspaceMemberships.AddRange(
            new WorkspaceMembershipEntity { Id = 100, UserId = 1, SchoolId = 1, WorkspaceType = WorkspaceType.School },
            new WorkspaceMembershipEntity { Id = 101, UserId = 2, SchoolId = 1, WorkspaceType = WorkspaceType.School },
            new WorkspaceMembershipEntity { Id = 102, UserId = 3, SchoolId = 1, WorkspaceType = WorkspaceType.School });
        db.StudentEntities.AddRange(
            new StudentEntity { Id = 1, SchoolId = 1, FirstName = "One", LastName = "Student", Status = AccountStatus.Active },
            new StudentEntity { Id = 2, SchoolId = 1, FirstName = "Two", LastName = "Student", Status = AccountStatus.Active },
            new StudentEntity { Id = 3, SchoolId = 2, FirstName = "Three", LastName = "Student", Status = AccountStatus.Active },
            new StudentEntity { Id = 4, UserId = 4, SchoolId = 1, FirstName = "Four", LastName = "Student", Status = AccountStatus.Active });
        db.StudentAssignments.AddRange(
            new StudentAssignmentEntity { Id = 1, StudentId = 1, TeacherId = 10, IsSchool = true },
            new StudentAssignmentEntity { Id = 2, StudentId = 1, TeacherId = 11, IsSchool = true },
            new StudentAssignmentEntity { Id = 3, StudentId = 3, TeacherId = 10, IsSchool = true },
            new StudentAssignmentEntity { Id = 4, StudentId = 4, TeacherId = 10, IsSchool = true });
        db.Courses.Add(new CourseEntity { Id = 1, SchoolId = 1, Name = "Math" });
        db.Set<StudentCourseAssignmentEntity>().Add(new StudentCourseAssignmentEntity { StudentId = 4, CourseId = 1 });
        db.Homeworks.AddRange(
            new HomeworkEntity { Id = 41, StudentId = 1, TeacherId = 10, SchoolId = 1, Title = "Assigned homework" },
            new HomeworkEntity { Id = 42, StudentId = 2, TeacherId = 10, SchoolId = 1, Title = "Unassigned homework" },
            new HomeworkEntity { Id = 43, StudentId = 3, TeacherId = 10, SchoolId = 2, Title = "Other school homework" },
            new HomeworkEntity { Id = 44, StudentId = 4, TeacherId = 10, SchoolId = 1, CourseId = 1, Title = "Student homework" });
        db.StudentNotes.AddRange(
            new StudentNoteEntity { Id = 1, StudentId = 1, TeacherId = 10, Content = "Own note" },
            new StudentNoteEntity { Id = 2, StudentId = 1, TeacherId = 11, Content = "Private note" });
        await db.SaveChangesAsync();
        _seeded = true;
    }
}

public sealed class TestHomeworkFileStorage : IHomeworkFileStorage
{
    private readonly Dictionary<string, byte[]> _files = new();

    public async Task<StoredHomeworkFile> SaveAsync(IFormFile file, CancellationToken cancellationToken = default)
    {
        await using var stream = new MemoryStream();
        await file.CopyToAsync(stream, cancellationToken);
        var storageName = $"{Guid.NewGuid():N}.jpg";
        var bytes = stream.ToArray();
        _files.Add(storageName, bytes);
        return new StoredHomeworkFile(storageName, Path.GetFileName(file.FileName), "image/jpeg", bytes.Length);
    }

    public Stream OpenRead(string storageName) =>
        new MemoryStream(_files.TryGetValue(storageName, out var bytes) ? bytes : throw new FileNotFoundException());

    public Task DeleteAsync(string storageName)
    {
        _files.Remove(storageName);
        return Task.CompletedTask;
    }
}

public class TestAuthenticationHandler : AuthenticationHandler<AuthenticationSchemeOptions>
{
    public const string SchemeName = "StudentManagementTest";
    public const string UserIdHeader = "X-Test-User";
    public const string RoleHeader = "X-Test-Role";
    public const string MembershipHeader = "X-Test-Membership";

    public TestAuthenticationHandler(
        IOptionsMonitor<AuthenticationSchemeOptions> options,
        ILoggerFactory logger,
        UrlEncoder encoder)
        : base(options, logger, encoder)
    {
    }

    protected override Task<AuthenticateResult> HandleAuthenticateAsync()
    {
        if (!int.TryParse(Request.Headers[UserIdHeader], out var userId) ||
            !Enum.TryParse<UserRole>(Request.Headers[RoleHeader], out var role) ||
            !int.TryParse(Request.Headers[MembershipHeader], out var membershipId))
            return Task.FromResult(AuthenticateResult.NoResult());

        var claims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, userId.ToString()),
            new Claim(ClaimTypes.Role, role.ToString()),
            new Claim("workspaceMembershipId", membershipId.ToString())
        };
        var identity = new ClaimsIdentity(claims, SchemeName);
        var ticket = new AuthenticationTicket(new ClaimsPrincipal(identity), SchemeName);
        return Task.FromResult(AuthenticateResult.Success(ticket));
    }
}
