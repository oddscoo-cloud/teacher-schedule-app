namespace backend.Tests.Services;

using backend.Core.Entities;
using backend.Data;
using backend.Infrastructure.Services;
using Microsoft.EntityFrameworkCore;

public class LessonServiceTests
{
    [Fact]
    public async Task ChangeStatusAsync_AppliesFiveHourBoundaryAndRecordsOnlyEligibleCancellations()
    {
        await using var db = CreateContext();
        await SeedWorkspaceAsync(db);
        var clock = new AdjustableTimeProvider(new DateTimeOffset(2026, 1, 1, 10, 0, 0, TimeSpan.Zero));
        var service = CreateLessonService(db, clock);
        var finance = CreateFinanceService(db);
        await finance.SetStandardPriceAsync(2, UserRole.Teacher, 101, 1, 80m,
            new DateTimeOffset(2025, 1, 1, 0, 0, 0, TimeSpan.Zero));

        var exact = await CreateLessonAsync(service, new DateTimeOffset(2026, 1, 1, 15, 0, 0, TimeSpan.Zero));
        var outside = await CreateLessonAsync(service, new DateTimeOffset(2026, 1, 1, 15, 0, 1, TimeSpan.Zero));
        var inside = await CreateLessonAsync(service, new DateTimeOffset(2026, 1, 1, 15, 0, 0, TimeSpan.Zero));

        Assert.True(await service.ChangeStatusAsync(2, UserRole.Teacher, 101, exact.Id, LessonStatus.Cancelled));
        Assert.Empty(await db.StudentLessonCharges.ToListAsync());
        Assert.True(await service.ChangeStatusAsync(2, UserRole.Teacher, 101, outside.Id, LessonStatus.Cancelled));
        Assert.Empty(await db.StudentLessonCharges.ToListAsync());

        clock.Set(new DateTimeOffset(2026, 1, 1, 10, 0, 1, TimeSpan.Zero));
        Assert.True(await service.ChangeStatusAsync(2, UserRole.Teacher, 101, inside.Id, LessonStatus.Cancelled));
        var charge = Assert.Single(await db.StudentLessonCharges.ToListAsync());
        Assert.Equal(inside.Id, charge.SourceLessonId);
        Assert.Equal(StudentLessonChargeType.LateCancellation, charge.ChargeType);
        Assert.Equal(new DateTime(2026, 1, 1, 10, 0, 1, DateTimeKind.Utc), charge.ChargedAt.UtcDateTime);
    }

    [Fact]
    public async Task RescheduleAsync_PreservesHistoryAndDoesNotCreateFinancialCharge()
    {
        await using var db = CreateContext();
        await SeedWorkspaceAsync(db);
        var service = CreateLessonService(db, new AdjustableTimeProvider(DateTimeOffset.UtcNow));
        var startsAt = DateTimeOffset.UtcNow.AddDays(1);
        var original = await CreateLessonAsync(service, startsAt);

        var replacement = await service.RescheduleAsync(
            2, UserRole.Teacher, 101, original.Id, startsAt.AddDays(2), "Student requested a new time");

        Assert.NotNull(replacement);
        Assert.Equal(original.Id, replacement.RescheduledFromId);
        Assert.Equal(LessonStatus.Rescheduled,
            (await db.LessonEvents.SingleAsync(item => item.Id == original.Id)).Status);
        Assert.Single(await db.LessonReschedules.ToListAsync());
        await Assert.ThrowsAsync<LessonConflictException>(() => service.RescheduleAsync(
            2, UserRole.Teacher, 101, original.Id, startsAt.AddDays(3), "Duplicate retry"));
        Assert.Equal(2, await db.LessonEvents.CountAsync());
        Assert.Empty(await db.StudentLessonCharges.ToListAsync());
    }

    [Fact]
    public async Task CreateSeriesAsync_PersistsLocalWeeklyOccurrencesAcrossDstAndKeepsLocalTime()
    {
        await using var db = CreateContext();
        await SeedWorkspaceAsync(db);
        var service = CreateLessonService(db, new AdjustableTimeProvider(new DateTimeOffset(2026, 2, 1, 0, 0, 0, TimeSpan.Zero)));

        var series = await service.CreateSeriesAsync(
            2, UserRole.Teacher, 101, 1, null, null, DayOfWeek.Sunday, TimeSpan.FromHours(9), 60,
            "Weekly lesson", null, new DateTime(2026, 3, 1), new DateTime(2026, 3, 22), "America/New_York");

        Assert.NotNull(series);
        var lessons = await db.LessonEvents.Where(item => item.SeriesId == series.Id)
            .OrderBy(item => item.ScheduledAtUtc).ToListAsync();
        Assert.Equal(4, lessons.Count);
        var zone = TimeZoneInfo.FindSystemTimeZoneById("America/New_York");
        var localStarts = lessons.Select(item => TimeZoneInfo.ConvertTime(
            new DateTimeOffset(DateTime.SpecifyKind(item.ScheduledAtUtc, DateTimeKind.Utc)), zone)).ToList();
        Assert.Equal(new[] { 1, 8, 15, 22 }, localStarts.Select(item => item.Day).ToArray());
        Assert.All(localStarts, item => Assert.Equal(9, item.Hour));
        Assert.Equal(TimeSpan.FromDays(7) - TimeSpan.FromHours(1),
            lessons[1].ScheduledAtUtc - lessons[0].ScheduledAtUtc);
    }

    [Fact]
    public async Task UpdateSeriesAsync_ChangesOnlyFutureScheduledInstances()
    {
        await using var db = CreateContext();
        await SeedWorkspaceAsync(db);
        var clock = new AdjustableTimeProvider(new DateTimeOffset(2026, 3, 10, 12, 0, 0, TimeSpan.Zero));
        var service = CreateLessonService(db, clock);
        var series = await service.CreateSeriesAsync(
            2, UserRole.Teacher, 101, 1, null, null, DayOfWeek.Sunday, TimeSpan.FromHours(9), 60,
            "Original", null, new DateTime(2026, 3, 1), new DateTime(2026, 3, 29), "America/New_York");
        Assert.NotNull(series);
        var firstOccurrence = await db.LessonEvents.SingleAsync(item => item.SeriesInstanceIndex == 0);
        firstOccurrence.Status = LessonStatus.Completed;
        await db.SaveChangesAsync();

        Assert.True(await service.UpdateSeriesAsync(
            2, UserRole.Teacher, 101, series.Id, DayOfWeek.Thursday, TimeSpan.FromHours(11), 45,
            "Updated", "New description", "America/New_York"));

        var instances = await db.LessonEvents.Where(item => item.SeriesId == series.Id)
            .OrderBy(item => item.SeriesInstanceIndex).ToListAsync();
        Assert.Equal(LessonStatus.Completed, instances[0].Status);
        Assert.Equal("Original", instances[0].Title);
        Assert.Equal("Original", instances[1].Title);
        Assert.Equal("Updated", instances[2].Title);
        var updatedLocal = TimeZoneInfo.ConvertTime(
            new DateTimeOffset(DateTime.SpecifyKind(instances[2].ScheduledAtUtc, DateTimeKind.Utc)),
            TimeZoneInfo.FindSystemTimeZoneById("America/New_York"));
        Assert.Equal(new DateTime(2026, 3, 19, 11, 0, 0), updatedLocal.DateTime);
        Assert.Equal(45, instances[2].Duration.TotalMinutes);
    }

    [Fact]
    public async Task CreateSeriesAsync_ResolvesInvalidAndAmbiguousLocalTimesDeterministically()
    {
        await using var db = CreateContext();
        await SeedWorkspaceAsync(db);
        var service = CreateLessonService(db, new AdjustableTimeProvider(DateTimeOffset.UtcNow));
        const string zoneId = "America/New_York";

        var spring = await service.CreateSeriesAsync(
            2, UserRole.Teacher, 101, 1, null, null, DayOfWeek.Sunday,
            TimeSpan.FromHours(2).Add(TimeSpan.FromMinutes(30)), 60, "Spring", null,
            new DateTime(2026, 3, 8), new DateTime(2026, 3, 8), zoneId);
        var fall = await service.CreateSeriesAsync(
            2, UserRole.Teacher, 101, 1, null, null, DayOfWeek.Sunday,
            TimeSpan.FromHours(1).Add(TimeSpan.FromMinutes(30)), 60, "Fall", null,
            new DateTime(2026, 11, 1), new DateTime(2026, 11, 1), zoneId);
        var springLesson = await db.LessonEvents.SingleAsync(item => item.SeriesId == spring!.Id);
        var fallLesson = await db.LessonEvents.SingleAsync(item => item.SeriesId == fall!.Id);
        var zone = TimeZoneInfo.FindSystemTimeZoneById(zoneId);
        var springLocal = TimeZoneInfo.ConvertTime(
            new DateTimeOffset(DateTime.SpecifyKind(springLesson.ScheduledAtUtc, DateTimeKind.Utc)), zone);
        var fallLocal = TimeZoneInfo.ConvertTime(
            new DateTimeOffset(DateTime.SpecifyKind(fallLesson.ScheduledAtUtc, DateTimeKind.Utc)), zone);

        Assert.Equal(new DateTime(2026, 3, 8, 3, 0, 0), springLocal.DateTime);
        Assert.Equal(new DateTime(2026, 11, 1, 1, 30, 0), fallLocal.DateTime);
        Assert.Equal(TimeSpan.FromHours(-4), fallLocal.Offset);
    }

    private static async Task<LessonEntity> CreateLessonAsync(LessonService service, DateTimeOffset startsAt)
    {
        var lesson = await service.CreateLessonAsync(
            2, UserRole.Teacher, 101, 1, null, null, "Lesson", null, startsAt, 60, "UTC");
        return Assert.IsType<LessonEntity>(lesson);
    }

    private static LessonService CreateLessonService(AppDbContext db, TimeProvider clock) =>
        new(db, new StudentWorkspaceAccessService(db), CreateFinanceService(db), clock);

    private static StudentFinanceService CreateFinanceService(AppDbContext db) =>
        new(db, new StudentWorkspaceAccessService(db));

    private static AppDbContext CreateContext()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        return new AppDbContext(options);
    }

    private static async Task SeedWorkspaceAsync(AppDbContext db)
    {
        db.Schools.Add(new SchoolEntity { Id = 1, Name = "School" });
        db.UserEntities.Add(new UserEntity
        {
            Id = 2, Email = "teacher@example.test", PasswordHash = "hash",
            Role = UserRole.Teacher, Status = AccountStatus.Active
        });
        db.TeacherEntities.Add(new TeacherEntity
        {
            Id = 10, UserId = 2, SchoolId = 1, FirstName = "Ada", LastName = "Teacher"
        });
        db.WorkspaceMemberships.Add(new WorkspaceMembershipEntity
        {
            Id = 101, UserId = 2, SchoolId = 1, WorkspaceType = WorkspaceType.School
        });
        db.StudentEntities.Add(new StudentEntity
        {
            Id = 1, SchoolId = 1, FirstName = "Sam", LastName = "Student", Status = AccountStatus.Active
        });
        db.StudentAssignments.Add(new StudentAssignmentEntity
        {
            Id = 1, StudentId = 1, TeacherId = 10, IsSchool = true
        });
        await db.SaveChangesAsync();
    }

    private sealed class AdjustableTimeProvider(DateTimeOffset currentTime) : TimeProvider
    {
        private DateTimeOffset _currentTime = currentTime;
        public override DateTimeOffset GetUtcNow() => _currentTime;
        public void Set(DateTimeOffset currentTime) => _currentTime = currentTime;
    }
}
