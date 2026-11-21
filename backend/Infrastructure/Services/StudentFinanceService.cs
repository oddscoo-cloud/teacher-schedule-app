namespace backend.Infrastructure.Services;

using backend.Core.Entities;
using backend.Data;
using Microsoft.EntityFrameworkCore;
using System.Data;

public enum FinanceWriteStatus
{
    Created,
    Existing,
    Skipped,
    NotFound,
    Conflict,
    Invalid
}

public sealed record FinanceWriteResult<T>(
    FinanceWriteStatus Status,
    T? Value = default,
    string? Error = null);

public sealed record StudentFinanceSnapshot(
    decimal? CurrentPrice,
    IReadOnlyList<StudentPriceHistoryEntity> PriceHistory,
    IReadOnlyList<StudentPaymentEntity> Payments,
    IReadOnlyList<StudentLessonChargeEntity> LessonCharges,
    int PaidLessonCount,
    int ChargedLessonCount,
    int LessonBalance,
    decimal PaymentsTotal,
    decimal ChargedAmount,
    decimal MoneyBalance,
    bool LessonChargesIntegrated);

public sealed class StudentFinanceService
{
    private const decimal MaximumAmount = 9999999999999999.99m;
    private readonly AppDbContext _db;
    private readonly StudentWorkspaceAccessService _workspaceAccess;

    public StudentFinanceService(AppDbContext db, StudentWorkspaceAccessService workspaceAccess)
    {
        _db = db;
        _workspaceAccess = workspaceAccess;
    }

    public async Task<StudentFinanceSnapshot?> GetFinanceAsync(
        int userId,
        UserRole role,
        int membershipId,
        int studentId)
    {
        if (!await CanAccessStudentAsync(userId, role, membershipId, studentId))
            return null;

        var prices = await _db.StudentPriceHistory
            .AsNoTracking()
            .Where(item => item.StudentId == studentId)
            .OrderBy(item => item.EffectiveFrom)
            .ToListAsync();
        var payments = await _db.StudentPayments
            .AsNoTracking()
            .Where(item => item.StudentId == studentId)
            .Include(item => item.Adjustments)
            .OrderBy(item => item.PaidAt)
            .ThenBy(item => item.Id)
            .ToListAsync();
        var charges = await _db.StudentLessonCharges
            .AsNoTracking()
            .Where(item => item.StudentId == studentId)
            .OrderBy(item => item.LessonStartsAt)
            .ThenBy(item => item.Id)
            .ToListAsync();

        var paidLessons = payments.Sum(payment =>
            payment.PaidLessonCount + payment.Adjustments.Sum(adjustment => adjustment.PaidLessonCountDelta));
        var paymentTotal = payments.Sum(payment =>
            payment.Amount + payment.Adjustments.Sum(adjustment => adjustment.AmountDelta));
        var chargedLessons = charges.Count;
        var chargedAmount = charges.Sum(charge => charge.Amount);
        return new StudentFinanceSnapshot(
            prices.LastOrDefault()?.Amount,
            prices,
            payments,
            charges,
            paidLessons,
            chargedLessons,
            paidLessons - chargedLessons,
            paymentTotal,
            chargedAmount,
            chargedAmount - paymentTotal,
            LessonChargesIntegrated: true);
    }

    public async Task<FinanceWriteResult<StudentLessonChargeEntity>> RecordLessonChargeAsync(
        int userId,
        UserRole role,
        int membershipId,
        int studentId,
        int sourceLessonId,
        StudentLessonChargeType chargeType,
        DateTimeOffset lessonStartsAt,
        DateTimeOffset? cancelledAt = null)
    {
        if (sourceLessonId <= 0 || lessonStartsAt == default ||
            !Enum.IsDefined(chargeType))
            return Invalid<StudentLessonChargeEntity>("A valid lesson id, start time, and charge type are required.");
        if (chargeType == StudentLessonChargeType.LateCancellation &&
            (cancelledAt == null || cancelledAt.Value == default || cancelledAt.Value > lessonStartsAt))
            return Invalid<StudentLessonChargeEntity>("A late cancellation must have a cancellation time at or before lesson start.");
        if (chargeType == StudentLessonChargeType.Completed && cancelledAt != null)
            return Invalid<StudentLessonChargeEntity>("Completed lessons cannot have a cancellation time.");
        if (!await CanAccessStudentAsync(userId, role, membershipId, studentId))
            return NotFound<StudentLessonChargeEntity>();

        var startsAt = lessonStartsAt.ToUniversalTime();
        var cancelledTime = cancelledAt?.ToUniversalTime();
        if (chargeType == StudentLessonChargeType.LateCancellation &&
            !LessonChargePolicy.IsLateCancellation(startsAt, cancelledTime!.Value))
            return new FinanceWriteResult<StudentLessonChargeEntity>(FinanceWriteStatus.Skipped);

        var existing = await _db.StudentLessonCharges.SingleOrDefaultAsync(item =>
            item.StudentId == studentId && item.SourceLessonId == sourceLessonId);
        if (existing != null)
            return existing.ChargeType == chargeType &&
                   existing.LessonStartsAt == startsAt &&
                   existing.ChargedAt == (chargeType == StudentLessonChargeType.Completed ? startsAt : cancelledTime)
                ? Existing(existing)
                : Conflict<StudentLessonChargeEntity>("This lesson already has a different financial charge.");

        var price = await _db.StudentPriceHistory
            .Where(item => item.StudentId == studentId && item.EffectiveFrom <= startsAt)
            .OrderByDescending(item => item.EffectiveFrom)
            .Select(item => (decimal?)item.Amount)
            .FirstOrDefaultAsync();
        if (!price.HasValue)
            return Invalid<StudentLessonChargeEntity>("A standard price effective at the lesson start is required.");

        var charge = new StudentLessonChargeEntity
        {
            StudentId = studentId,
            SourceLessonId = sourceLessonId,
            ChargeType = chargeType,
            LessonStartsAt = startsAt,
            ChargedAt = chargeType == StudentLessonChargeType.Completed ? startsAt : cancelledTime!.Value,
            Amount = price.Value,
            RecordedByUserId = userId
        };

        await using var transaction = _db.Database.IsRelational()
            ? await _db.Database.BeginTransactionAsync(IsolationLevel.Serializable)
            : null;
        _db.StudentLessonCharges.Add(charge);
        try
        {
            await _db.SaveChangesAsync();
            if (transaction != null)
                await transaction.CommitAsync();
            return Created(charge);
        }
        catch (DbUpdateException)
        {
            if (transaction != null)
                await transaction.RollbackAsync();
            _db.Entry(charge).State = EntityState.Detached;
            var raced = await _db.StudentLessonCharges.SingleOrDefaultAsync(item =>
                item.StudentId == studentId && item.SourceLessonId == sourceLessonId);
            return raced?.ChargeType == chargeType &&
                   raced.LessonStartsAt == startsAt &&
                   raced.ChargedAt == charge.ChargedAt
                ? Existing(raced)
                : Conflict<StudentLessonChargeEntity>("This lesson already has a different financial charge.");
        }
    }

    public async Task<FinanceWriteResult<StudentPriceHistoryEntity>> SetStandardPriceAsync(
        int userId,
        UserRole role,
        int membershipId,
        int studentId,
        decimal amount,
        DateTimeOffset? effectiveFrom = null)
    {
        if (!IsValidMoney(amount, allowZero: true))
            return Invalid<StudentPriceHistoryEntity>("Price must be a non-negative UAH amount with at most two decimal places.");

        if (!await CanAccessStudentAsync(userId, role, membershipId, studentId))
            return NotFound<StudentPriceHistoryEntity>();

        var effectiveAt = (effectiveFrom ?? DateTimeOffset.UtcNow).ToUniversalTime();
        var atThisTime = await _db.StudentPriceHistory
            .SingleOrDefaultAsync(item => item.StudentId == studentId && item.EffectiveFrom == effectiveAt);
        if (atThisTime != null)
            return atThisTime.Amount == amount
                ? Existing(atThisTime)
                : Conflict<StudentPriceHistoryEntity>("A price already exists at that effective time.");

        var latest = await _db.StudentPriceHistory
            .Where(item => item.StudentId == studentId)
            .OrderByDescending(item => item.EffectiveFrom)
            .FirstOrDefaultAsync();
        if (latest != null && effectiveAt <= latest.EffectiveFrom)
        {
            return latest.EffectiveFrom == effectiveAt && latest.Amount == amount
                ? Existing(latest)
                : Conflict<StudentPriceHistoryEntity>("Price history is append-only; effective time must be later than the latest price.");
        }

        var price = new StudentPriceHistoryEntity
        {
            StudentId = studentId,
            Amount = amount,
            EffectiveFrom = effectiveAt,
            RecordedAt = DateTimeOffset.UtcNow,
            RecordedByUserId = userId
        };
        _db.StudentPriceHistory.Add(price);
        try
        {
            await _db.SaveChangesAsync();
            return Created(price);
        }
        catch (DbUpdateException)
        {
            _db.Entry(price).State = EntityState.Detached;
            var duplicate = await _db.StudentPriceHistory
                .SingleOrDefaultAsync(item => item.StudentId == studentId && item.EffectiveFrom == effectiveAt);
            return duplicate?.Amount == amount
                ? Existing(duplicate)
                : Conflict<StudentPriceHistoryEntity>("A price already exists at that effective time.");
        }
    }

    public async Task<FinanceWriteResult<StudentPaymentEntity>> RecordPaymentAsync(
        int userId,
        UserRole role,
        int membershipId,
        int studentId,
        decimal amount,
        int paidLessonCount,
        DateTimeOffset paidAt,
        string? idempotencyKey)
    {
        if (!IsValidMoney(amount, allowZero: false))
            return Invalid<StudentPaymentEntity>("Payment must be a positive UAH amount with at most two decimal places.");
        if (paidLessonCount < 0)
            return Invalid<StudentPaymentEntity>("Paid lesson count cannot be negative.");
        if (string.IsNullOrWhiteSpace(idempotencyKey) || idempotencyKey.Trim().Length > 100)
            return Invalid<StudentPaymentEntity>("A non-empty idempotency key of at most 100 characters is required.");
        if (paidAt == default)
            return Invalid<StudentPaymentEntity>("Payment date is required.");
        if (!await CanAccessStudentAsync(userId, role, membershipId, studentId))
            return NotFound<StudentPaymentEntity>();

        var key = idempotencyKey.Trim();
        var existing = await _db.StudentPayments
            .SingleOrDefaultAsync(item => item.StudentId == studentId && item.IdempotencyKey == key);
        if (existing != null)
            return existing.Amount == amount && existing.PaidLessonCount == paidLessonCount &&
                   existing.PaidAt == paidAt.ToUniversalTime()
                ? Existing(existing)
                : Conflict<StudentPaymentEntity>("This idempotency key was already used for a different payment.");

        var payment = new StudentPaymentEntity
        {
            StudentId = studentId,
            Amount = amount,
            PaidLessonCount = paidLessonCount,
            PaidAt = paidAt.ToUniversalTime(),
            RecordedAt = DateTimeOffset.UtcNow,
            RecordedByUserId = userId,
            IdempotencyKey = key
        };

        await using var transaction = _db.Database.IsRelational()
            ? await _db.Database.BeginTransactionAsync(IsolationLevel.Serializable)
            : null;
        _db.StudentPayments.Add(payment);
        try
        {
            await _db.SaveChangesAsync();
            if (transaction != null)
                await transaction.CommitAsync();
            return Created(payment);
        }
        catch (DbUpdateException)
        {
            if (transaction != null)
                await transaction.RollbackAsync();
            _db.Entry(payment).State = EntityState.Detached;
            var raced = await _db.StudentPayments
                .SingleOrDefaultAsync(item => item.StudentId == studentId && item.IdempotencyKey == key);
            return raced?.Amount == amount && raced.PaidLessonCount == paidLessonCount &&
                   raced.PaidAt == paidAt.ToUniversalTime()
                ? Existing(raced)
                : Conflict<StudentPaymentEntity>("This idempotency key was already used for a different payment.");
        }
    }

    public async Task<FinanceWriteResult<StudentPaymentAdjustmentEntity>> AdjustPaymentAsync(
        int userId,
        UserRole role,
        int membershipId,
        int studentId,
        int paymentId,
        decimal amountDelta,
        int paidLessonCountDelta,
        string? reason,
        string? idempotencyKey)
    {
        if (!IsValidMoney(amountDelta, allowZero: true, allowNegative: true))
            return Invalid<StudentPaymentAdjustmentEntity>("Adjustment must be a UAH amount with at most two decimal places.");
        if (amountDelta == 0m && paidLessonCountDelta == 0)
            return Invalid<StudentPaymentAdjustmentEntity>("An adjustment must change the amount or lesson count.");
        if (string.IsNullOrWhiteSpace(reason) || reason.Trim().Length > 500)
            return Invalid<StudentPaymentAdjustmentEntity>("A reason of 1 to 500 characters is required.");
        if (string.IsNullOrWhiteSpace(idempotencyKey) || idempotencyKey.Trim().Length > 100)
            return Invalid<StudentPaymentAdjustmentEntity>("A non-empty idempotency key of at most 100 characters is required.");
        if (!await CanAccessStudentAsync(userId, role, membershipId, studentId))
            return NotFound<StudentPaymentAdjustmentEntity>();

        await using var transaction = _db.Database.IsRelational()
            ? await _db.Database.BeginTransactionAsync(IsolationLevel.Serializable)
            : null;
        var payment = await _db.StudentPayments
            .Include(item => item.Adjustments)
            .SingleOrDefaultAsync(item => item.Id == paymentId && item.StudentId == studentId);
        if (payment == null)
            return NotFound<StudentPaymentAdjustmentEntity>();

        var key = idempotencyKey.Trim();
        var existing = payment.Adjustments.SingleOrDefault(item => item.IdempotencyKey == key);
        if (existing != null)
            return existing.AmountDelta == amountDelta &&
                   existing.PaidLessonCountDelta == paidLessonCountDelta &&
                   existing.Reason == reason.Trim()
                ? Existing(existing)
                : Conflict<StudentPaymentAdjustmentEntity>("This idempotency key was already used for a different adjustment.");

        var netAmount = payment.Amount + payment.Adjustments.Sum(item => item.AmountDelta) + amountDelta;
        var netLessons = payment.PaidLessonCount +
                         payment.Adjustments.Sum(item => item.PaidLessonCountDelta) +
                         paidLessonCountDelta;
        if (netAmount < 0m || netLessons < 0)
            return Invalid<StudentPaymentAdjustmentEntity>("Adjustments cannot reduce a payment below zero.");

        var adjustment = new StudentPaymentAdjustmentEntity
        {
            PaymentId = paymentId,
            AmountDelta = amountDelta,
            PaidLessonCountDelta = paidLessonCountDelta,
            Reason = reason.Trim(),
            RecordedAt = DateTimeOffset.UtcNow,
            RecordedByUserId = userId,
            IdempotencyKey = key
        };

        _db.StudentPaymentAdjustments.Add(adjustment);
        try
        {
            await _db.SaveChangesAsync();
            if (transaction != null)
                await transaction.CommitAsync();
            return Created(adjustment);
        }
        catch (DbUpdateException)
        {
            if (transaction != null)
                await transaction.RollbackAsync();
            _db.Entry(adjustment).State = EntityState.Detached;
            var raced = await _db.StudentPaymentAdjustments
                .SingleOrDefaultAsync(item => item.PaymentId == paymentId && item.IdempotencyKey == key);
            return raced?.AmountDelta == amountDelta &&
                   raced.PaidLessonCountDelta == paidLessonCountDelta &&
                   raced.Reason == reason.Trim()
                ? Existing(raced)
                : Conflict<StudentPaymentAdjustmentEntity>("This idempotency key was already used for a different adjustment.");
        }
    }

    private async Task<bool> CanAccessStudentAsync(
        int userId,
        UserRole role,
        int membershipId,
        int studentId)
    {
        var access = await _workspaceAccess.ResolveAsync(userId, role, membershipId);
        return access != null && await _workspaceAccess.VisibleStudents(access)
            .AnyAsync(student => student.Id == studentId);
    }

    private static bool IsValidMoney(decimal amount, bool allowZero, bool allowNegative = false) =>
        (allowZero ? (allowNegative || amount >= 0m) : amount > 0m) &&
        (!allowNegative || amount >= -MaximumAmount) &&
        amount <= MaximumAmount &&
        decimal.Round(amount, 2) == amount;

    private static FinanceWriteResult<T> Created<T>(T value) => new(FinanceWriteStatus.Created, value);
    private static FinanceWriteResult<T> Existing<T>(T value) => new(FinanceWriteStatus.Existing, value);
    private static FinanceWriteResult<T> NotFound<T>() => new(FinanceWriteStatus.NotFound);
    private static FinanceWriteResult<T> Conflict<T>(string message) => new(FinanceWriteStatus.Conflict, Error: message);
    private static FinanceWriteResult<T> Invalid<T>(string message) => new(FinanceWriteStatus.Invalid, Error: message);
}
