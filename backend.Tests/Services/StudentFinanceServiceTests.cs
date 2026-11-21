namespace backend.Tests.Services;

using backend.Core.Entities;
using backend.Data;
using backend.Infrastructure.Services;
using Microsoft.EntityFrameworkCore;

public class StudentFinanceServiceTests
{
    [Fact]
    public async Task FinanceAsync_CalculatesIndependentExactBalancesAndKeepsRateHistory()
    {
        await using var db = CreateContext();
        await SeedWorkspaceDataAsync(db);
        var service = CreateService(db);
        var firstRateTime = new DateTimeOffset(2026, 1, 1, 0, 0, 0, TimeSpan.Zero);
        var nextRateTime = firstRateTime.AddDays(30);

        var firstRate = await service.SetStandardPriceAsync(
            2, UserRole.Teacher, 101, 1, 125.35m, firstRateTime);
        var secondRate = await service.SetStandardPriceAsync(
            2, UserRole.Teacher, 101, 1, 150.25m, nextRateTime);
        Assert.Equal(FinanceWriteStatus.Created, firstRate.Status);
        Assert.Equal(FinanceWriteStatus.Created, secondRate.Status);
        Assert.Equal(FinanceWriteStatus.Existing,
            (await service.SetStandardPriceAsync(2, UserRole.Teacher, 101, 1, 125.35m, firstRateTime)).Status);

        var paymentTime = nextRateTime.AddDays(1);
        var payment = await service.RecordPaymentAsync(
            2, UserRole.Teacher, 101, 1, 100.25m, 3, paymentTime, "payment-1");
        Assert.Equal(FinanceWriteStatus.Created, payment.Status);
        Assert.NotNull(payment.Value);
        Assert.Equal(FinanceWriteStatus.Created,
            (await service.RecordPaymentAsync(
                2, UserRole.Teacher, 101, 1, 50.00m, 2, paymentTime.AddDays(1), "payment-2")).Status);

        var correction = await service.AdjustPaymentAsync(
            2, UserRole.Teacher, 101, 1, payment.Value.Id, -10.25m, -1, "Partial refund", "refund-1");
        Assert.Equal(FinanceWriteStatus.Created, correction.Status);
        Assert.Equal(FinanceWriteStatus.Existing,
            (await service.AdjustPaymentAsync(
                2, UserRole.Teacher, 101, 1, payment.Value.Id, -10.25m, -1, "Partial refund", "refund-1")).Status);

        var snapshot = await service.GetFinanceAsync(2, UserRole.Teacher, 101, 1);
        Assert.NotNull(snapshot);
        Assert.Equal(150.25m, snapshot.CurrentPrice);
        Assert.Equal(2, snapshot.PriceHistory.Count);
        Assert.Equal(4, snapshot.PaidLessonCount);
        Assert.Equal(4, snapshot.LessonBalance);
        Assert.Equal(140.00m, snapshot.PaymentsTotal);
        Assert.Equal(-140.00m, snapshot.MoneyBalance);
        Assert.False(snapshot.LessonChargesIntegrated);
    }

    [Fact]
    public async Task RecordLessonChargeAsync_UsesRateAtLessonTimeAndDeduplicatesByLesson()
    {
        await using var db = CreateContext();
        await SeedWorkspaceDataAsync(db);
        var service = CreateService(db);
        var oldRateStart = new DateTimeOffset(2026, 1, 1, 0, 0, 0, TimeSpan.Zero);
        var newRateStart = oldRateStart.AddDays(10);
        await service.SetStandardPriceAsync(2, UserRole.Teacher, 101, 1, 100m, oldRateStart);
        await service.SetStandardPriceAsync(2, UserRole.Teacher, 101, 1, 125.50m, newRateStart);

        var lessonStart = newRateStart.AddDays(1);
        var completed = await service.RecordLessonChargeAsync(
            2, UserRole.Teacher, 101, 1, 44, StudentLessonChargeType.Completed, lessonStart);
        Assert.Equal(FinanceWriteStatus.Created, completed.Status);
        Assert.Equal(125.50m, completed.Value!.Amount);
        Assert.Equal(FinanceWriteStatus.Existing,
            (await service.RecordLessonChargeAsync(
                2, UserRole.Teacher, 101, 1, 44, StudentLessonChargeType.Completed, lessonStart)).Status);
        Assert.Equal(FinanceWriteStatus.Conflict,
            (await service.RecordLessonChargeAsync(
                2, UserRole.Teacher, 101, 1, 44, StudentLessonChargeType.LateCancellation,
                lessonStart, lessonStart.AddHours(-1))).Status);

        var snapshot = await service.GetFinanceAsync(2, UserRole.Teacher, 101, 1);
        Assert.NotNull(snapshot);
        Assert.Equal(1, snapshot.ChargedLessonCount);
        Assert.Equal(125.50m, snapshot.ChargedAmount);
        Assert.Equal(125.50m, snapshot.MoneyBalance);
        Assert.Single(snapshot.LessonCharges);
    }

    [Fact]
    public async Task RecordLessonChargeAsync_SkipsCancellationAtFiveHoursAndChargesJustInsideBoundary()
    {
        await using var db = CreateContext();
        await SeedWorkspaceDataAsync(db);
        var service = CreateService(db);
        var lessonStart = new DateTimeOffset(2026, 4, 1, 10, 0, 0, TimeSpan.Zero);
        await service.SetStandardPriceAsync(2, UserRole.Teacher, 101, 1, 80m, lessonStart.AddDays(-1));

        Assert.Equal(FinanceWriteStatus.Skipped,
            (await service.RecordLessonChargeAsync(
                2, UserRole.Teacher, 101, 1, 45, StudentLessonChargeType.LateCancellation,
                lessonStart, lessonStart.AddHours(-5))).Status);
        Assert.Equal(FinanceWriteStatus.Skipped,
            (await service.RecordLessonChargeAsync(
                2, UserRole.Teacher, 101, 1, 46, StudentLessonChargeType.LateCancellation,
                lessonStart, lessonStart.AddHours(-5).AddTicks(-1))).Status);

        var charged = await service.RecordLessonChargeAsync(
            2, UserRole.Teacher, 101, 1, 47, StudentLessonChargeType.LateCancellation,
            lessonStart, lessonStart.AddHours(-5).AddTicks(1));
        Assert.Equal(FinanceWriteStatus.Created, charged.Status);
        Assert.Equal(80m, charged.Value!.Amount);
        var snapshot = await service.GetFinanceAsync(2, UserRole.Teacher, 101, 1);
        Assert.Equal(1, snapshot!.ChargedLessonCount);
        Assert.Equal(80m, snapshot.ChargedAmount);
    }

    [Fact]
    public async Task RecordLessonChargeAsync_RequiresAnEffectiveRateAndVisibleStudent()
    {
        await using var db = CreateContext();
        await SeedWorkspaceDataAsync(db);
        var service = CreateService(db);
        var lessonStart = new DateTimeOffset(2026, 4, 1, 10, 0, 0, TimeSpan.Zero);

        Assert.Equal(FinanceWriteStatus.NotFound,
            (await service.RecordLessonChargeAsync(
                2, UserRole.Teacher, 101, 2, 48, StudentLessonChargeType.Completed, lessonStart)).Status);
        Assert.Equal(FinanceWriteStatus.Invalid,
            (await service.RecordLessonChargeAsync(
                2, UserRole.Teacher, 101, 1, 49, StudentLessonChargeType.Completed, lessonStart)).Status);
    }

    [Fact]
    public async Task RecordPaymentAsync_RejectsInvalidPrecisionAndConflictingRetries()
    {
        await using var db = CreateContext();
        await SeedWorkspaceDataAsync(db);
        var service = CreateService(db);
        var paidAt = new DateTimeOffset(2026, 2, 1, 12, 0, 0, TimeSpan.Zero);

        Assert.Equal(FinanceWriteStatus.Invalid,
            (await service.RecordPaymentAsync(2, UserRole.Teacher, 101, 1, 1.001m, 1, paidAt, "fraction")).Status);
        Assert.Equal(FinanceWriteStatus.Invalid,
            (await service.RecordPaymentAsync(2, UserRole.Teacher, 101, 1, -0.01m, 1, paidAt, "negative")).Status);
        Assert.Equal(FinanceWriteStatus.Invalid,
            (await service.RecordPaymentAsync(2, UserRole.Teacher, 101, 1, 0.01m, -1, paidAt, "bad-count")).Status);

        Assert.Equal(FinanceWriteStatus.Created,
            (await service.RecordPaymentAsync(2, UserRole.Teacher, 101, 1, 9999999999999999.99m, 0, paidAt, "max")).Status);
        Assert.Equal(FinanceWriteStatus.Conflict,
            (await service.RecordPaymentAsync(2, UserRole.Teacher, 101, 1, 20m, 1, paidAt, "max")).Status);
        Assert.Equal(FinanceWriteStatus.Existing,
            (await service.RecordPaymentAsync(2, UserRole.Teacher, 101, 1, 9999999999999999.99m, 0, paidAt, "max")).Status);
    }

    [Fact]
    public async Task AdjustPaymentAsync_PreservesAuditAndCannotRefundBelowZero()
    {
        await using var db = CreateContext();
        await SeedWorkspaceDataAsync(db);
        var service = CreateService(db);
        var paidAt = new DateTimeOffset(2026, 3, 1, 12, 0, 0, TimeSpan.Zero);
        var payment = await service.RecordPaymentAsync(
            2, UserRole.Teacher, 101, 1, 50m, 2, paidAt, "payment");
        Assert.NotNull(payment.Value);

        Assert.Equal(FinanceWriteStatus.Invalid,
            (await service.AdjustPaymentAsync(
                2, UserRole.Teacher, 101, 1, payment.Value.Id, -50.01m, -2, "Over refund", "bad")).Status);
        Assert.Equal(FinanceWriteStatus.Created,
            (await service.AdjustPaymentAsync(
                2, UserRole.Teacher, 101, 1, payment.Value.Id, -5m, 0, "Refund", "refund")).Status);
        Assert.Equal(FinanceWriteStatus.Conflict,
            (await service.AdjustPaymentAsync(
                2, UserRole.Teacher, 101, 1, payment.Value.Id, -5m, 0, "Edited reason", "refund")).Status);
        Assert.Equal(FinanceWriteStatus.Existing,
            (await service.AdjustPaymentAsync(
                2, UserRole.Teacher, 101, 1, payment.Value.Id, -5m, 0, "Refund", "refund")).Status);
        Assert.Single(await db.StudentPaymentAdjustments.ToListAsync());
    }

    [Fact]
    public async Task FinanceAsync_RestrictsTeachersToAssignedStudentsAndKeepsWorkspaceIsolation()
    {
        await using var db = CreateContext();
        await SeedWorkspaceDataAsync(db);
        var service = CreateService(db);

        Assert.Null(await service.GetFinanceAsync(2, UserRole.Teacher, 101, 2));
        Assert.Null(await service.GetFinanceAsync(2, UserRole.Teacher, 101, 3));
        Assert.NotNull(await service.GetFinanceAsync(1, UserRole.SchoolAdmin, 100, 2));
        Assert.Null(await service.GetFinanceAsync(1, UserRole.SchoolAdmin, 100, 3));
        Assert.NotNull(await service.GetFinanceAsync(4, UserRole.Teacher, 103, 4));
        Assert.Null(await service.GetFinanceAsync(4, UserRole.Teacher, 103, 1));
    }

    [Fact]
    public void LateCancellationPolicy_UsesStrictFiveHourBoundary()
    {
        var start = new DateTimeOffset(2026, 4, 1, 10, 0, 0, TimeSpan.Zero);

        Assert.False(LessonChargePolicy.IsLateCancellation(start, start.AddHours(-5)));
        Assert.False(LessonChargePolicy.IsLateCancellation(start, start.AddHours(-5).AddTicks(-1)));
        Assert.True(LessonChargePolicy.IsLateCancellation(start, start.AddHours(-5).AddTicks(1)));
        Assert.True(LessonChargePolicy.IsLateCancellation(start, start));
        Assert.False(LessonChargePolicy.IsLateCancellation(start, start.AddTicks(1)));
    }

    private static StudentFinanceService CreateService(AppDbContext db) =>
        new(db, new StudentWorkspaceAccessService(db));

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
            new WorkspaceMembershipEntity { Id = 103, UserId = 4, WorkspaceType = WorkspaceType.Standalone });
        db.StudentEntities.AddRange(
            new StudentEntity { Id = 1, SchoolId = 1, FirstName = "Assigned", LastName = "Student", Status = AccountStatus.Active },
            new StudentEntity { Id = 2, SchoolId = 1, FirstName = "Unassigned", LastName = "Student", Status = AccountStatus.Active },
            new StudentEntity { Id = 3, SchoolId = 2, FirstName = "Other", LastName = "School", Status = AccountStatus.Active },
            new StudentEntity { Id = 4, StandaloneTeacherId = 12, FirstName = "Solo", LastName = "Student", Status = AccountStatus.Active });
        db.StudentAssignments.Add(new StudentAssignmentEntity
        {
            Id = 1,
            StudentId = 1,
            TeacherId = 10,
            IsSchool = true
        });
        await db.SaveChangesAsync();
    }
}
