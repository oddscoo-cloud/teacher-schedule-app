namespace backend.Tests.Integration;

using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using backend.Core.Entities;

public sealed class LessonsApiTests : IClassFixture<StudentManagementApiFactory>, IAsyncLifetime
{
    private readonly StudentManagementApiFactory _factory;

    public LessonsApiTests(StudentManagementApiFactory factory) => _factory = factory;

    public Task InitializeAsync() => _factory.SeedAsync();
    public Task DisposeAsync() => Task.CompletedTask;

    [Fact]
    public async Task LessonsEndpointsRequireAuthenticationAndScopeResultsToWorkspaceAndTeacher()
    {
        using var anonymous = _factory.CreateClient();
        using var teacher = CreateClient(2, UserRole.Teacher, 101);
        using var otherTeacher = CreateClient(3, UserRole.Teacher, 102);

        Assert.Equal(HttpStatusCode.Unauthorized,
            (await anonymous.GetAsync("/api/lessons?from=2027-01-01T00:00:00Z&to=2027-01-08T00:00:00Z")).StatusCode);

        var creation = await teacher.PostAsJsonAsync("/api/lessons", new
        {
            studentId = 1,
            title = "Scoped lesson",
            scheduledAt = "2027-01-04T10:00:00Z",
            durationMinutes = 60,
            timezone = "UTC"
        });
        Assert.Equal(HttpStatusCode.Created, creation.StatusCode);
        using var createdJson = JsonDocument.Parse(await creation.Content.ReadAsStringAsync());
        var lessonId = createdJson.RootElement.GetProperty("id").GetInt32();

        var ownLessons = await teacher.GetAsync("/api/lessons?from=2027-01-01T00:00:00Z&to=2027-01-08T00:00:00Z&status=Scheduled");
        var otherLessons = await otherTeacher.GetAsync("/api/lessons?from=2027-01-01T00:00:00Z&to=2027-01-08T00:00:00Z");
        var cancellation = await teacher.PutAsJsonAsync($"/api/lessons/{lessonId}/status", new
        {
            status = "Cancelled",
            reason = "Student requested a cancellation"
        });
        var cancelledLessons = await teacher.GetAsync("/api/lessons?from=2027-01-01T00:00:00Z&to=2027-01-08T00:00:00Z&status=Cancelled");
        var cancelledDetail = await teacher.GetAsync($"/api/lessons/{lessonId}");
        Assert.Equal(HttpStatusCode.OK, ownLessons.StatusCode);
        Assert.Equal(HttpStatusCode.OK, otherLessons.StatusCode);
        Assert.Equal(HttpStatusCode.NoContent, cancellation.StatusCode);
        Assert.Single(JsonDocument.Parse(await ownLessons.Content.ReadAsStringAsync()).RootElement.EnumerateArray());
        Assert.Empty(JsonDocument.Parse(await otherLessons.Content.ReadAsStringAsync()).RootElement.EnumerateArray());
        var cancelledRecord = Assert.Single(JsonDocument.Parse(await cancelledLessons.Content.ReadAsStringAsync()).RootElement.EnumerateArray());
        using var detail = JsonDocument.Parse(await cancelledDetail.Content.ReadAsStringAsync());
        Assert.Equal("Student requested a cancellation", detail.RootElement.GetProperty("statusReason").GetString());
        Assert.Equal("Cancelled", cancelledRecord.GetProperty("status").GetString());
    }

    [Fact]
    public async Task CreateLesson_RejectsStudentsOutsideTeacherAssignmentAndInvalidStatusFilter()
    {
        using var teacher = CreateClient(2, UserRole.Teacher, 101);
        var inaccessible = await teacher.PostAsJsonAsync("/api/lessons", new
        {
            studentId = 2,
            title = "Should not be created",
            scheduledAt = "2027-01-04T10:00:00Z",
            durationMinutes = 60,
            timezone = "UTC"
        });
        var invalidFilter = await teacher.GetAsync(
            "/api/lessons?from=2027-01-01T00:00:00Z&to=2027-01-08T00:00:00Z&status=not-a-status");

        Assert.Equal(HttpStatusCode.NotFound, inaccessible.StatusCode);
        Assert.Equal(HttpStatusCode.BadRequest, invalidFilter.StatusCode);
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
