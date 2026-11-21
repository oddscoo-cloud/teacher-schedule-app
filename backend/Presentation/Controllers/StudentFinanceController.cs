namespace backend.Presentation.Controllers;

using backend.Core.Entities;
using backend.Infrastructure.Services;
using backend.Presentation.DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

[ApiController]
[Route("api/students/{studentId:int}/finance")]
[Authorize]
public sealed class StudentFinanceController : AuthorizedWorkspaceControllerBase
{
    private readonly StudentFinanceService _finance;

    public StudentFinanceController(StudentFinanceService finance)
    {
        _finance = finance;
    }

    [HttpGet]
    public async Task<ActionResult<StudentFinanceResponseDto>> GetFinance(int studentId)
    {
        if (!TryGetRequester(out var requester))
            return Unauthorized();
        var snapshot = await _finance.GetFinanceAsync(
            requester.UserId, requester.Role, requester.MembershipId, studentId);
        return snapshot == null ? NotFound() : Ok(ToResponse(snapshot));
    }

    [HttpPost("prices")]
    public async Task<IActionResult> SetStandardPrice(int studentId, SetStudentPriceRequestDto request)
    {
        if (!TryGetRequester(out var requester))
            return Unauthorized();

        var result = await _finance.SetStandardPriceAsync(
            requester.UserId, requester.Role, requester.MembershipId, studentId, request.Amount);
        if (result.Status == FinanceWriteStatus.NotFound)
            return NotFound();
        if (result.Status == FinanceWriteStatus.Invalid)
            return BadRequest(new { message = result.Error });
        if (result.Status == FinanceWriteStatus.Conflict)
            return Conflict(new { message = result.Error });
        return result.Value == null ? StatusCode(500) : Ok(ToResponse(result.Value));
    }

    [HttpPost("payments")]
    public async Task<IActionResult> RecordPayment(int studentId, RecordStudentPaymentRequestDto request)
    {
        if (!TryGetRequester(out var requester))
            return Unauthorized();

        if (!request.PaidAt.HasValue)
            return BadRequest(new { message = "Payment date is required." });
        var result = await _finance.RecordPaymentAsync(
            requester.UserId,
            requester.Role,
            requester.MembershipId,
            studentId,
            request.Amount,
            request.PaidLessonCount,
            request.PaidAt.Value,
            request.IdempotencyKey);
        if (result.Status == FinanceWriteStatus.NotFound)
            return NotFound();
        if (result.Status == FinanceWriteStatus.Invalid)
            return BadRequest(new { message = result.Error });
        if (result.Status == FinanceWriteStatus.Conflict)
            return Conflict(new { message = result.Error });
        return result.Value == null ? StatusCode(500) : Ok(ToResponse(result.Value));
    }

    [HttpPost("payments/{paymentId:int}/adjustments")]
    public async Task<IActionResult> AdjustPayment(
        int studentId,
        int paymentId,
        AdjustStudentPaymentRequestDto request)
    {
        if (!TryGetRequester(out var requester))
            return Unauthorized();

        var result = await _finance.AdjustPaymentAsync(
            requester.UserId,
            requester.Role,
            requester.MembershipId,
            studentId,
            paymentId,
            request.AmountDelta,
            request.PaidLessonCountDelta,
            request.Reason,
            request.IdempotencyKey);
        if (result.Status == FinanceWriteStatus.NotFound)
            return NotFound();
        if (result.Status == FinanceWriteStatus.Invalid)
            return BadRequest(new { message = result.Error });
        if (result.Status == FinanceWriteStatus.Conflict)
            return Conflict(new { message = result.Error });
        return result.Value == null ? StatusCode(500) : Ok(ToResponse(result.Value));
    }

    private static StudentFinanceResponseDto ToResponse(StudentFinanceSnapshot snapshot) => new()
    {
        StandardPrice = snapshot.CurrentPrice,
        PaidLessonCount = snapshot.PaidLessonCount,
        ChargedLessonCount = snapshot.ChargedLessonCount,
        LessonBalance = snapshot.LessonBalance,
        PaymentsTotal = snapshot.PaymentsTotal,
        ChargedAmount = snapshot.ChargedAmount,
        MoneyBalance = snapshot.MoneyBalance,
        LessonChargesIntegrated = snapshot.LessonChargesIntegrated,
        PriceHistory = snapshot.PriceHistory.Select(ToResponse).ToList(),
        Payments = snapshot.Payments.Select(ToResponse).ToList(),
        Charges = snapshot.LessonCharges.Select(charge => new StudentLessonChargeResponseDto
        {
            Id = charge.Id,
            SourceLessonId = charge.SourceLessonId,
            Type = charge.ChargeType.ToString(),
            LessonStartsAt = charge.LessonStartsAt,
            ChargedAt = charge.ChargedAt,
            Amount = charge.Amount,
            RecordedByUserId = charge.RecordedByUserId
        }).ToList()
    };

    private static StudentPriceHistoryResponseDto ToResponse(StudentPriceHistoryEntity price) => new()
    {
        Id = price.Id,
        Amount = price.Amount,
        EffectiveFrom = price.EffectiveFrom,
        RecordedAt = price.RecordedAt,
        RecordedByUserId = price.RecordedByUserId
    };

    private static StudentPaymentResponseDto ToResponse(StudentPaymentEntity payment)
    {
        var adjustmentAmount = payment.Adjustments.Sum(item => item.AmountDelta);
        var adjustmentLessons = payment.Adjustments.Sum(item => item.PaidLessonCountDelta);
        return new StudentPaymentResponseDto
        {
            Id = payment.Id,
            Amount = payment.Amount,
            PaidLessonCount = payment.PaidLessonCount,
            PaidAt = payment.PaidAt,
            RecordedAt = payment.RecordedAt,
            RecordedByUserId = payment.RecordedByUserId,
            NetAmount = payment.Amount + adjustmentAmount,
            NetPaidLessonCount = payment.PaidLessonCount + adjustmentLessons,
            Adjustments = payment.Adjustments.OrderBy(item => item.RecordedAt).Select(item => new StudentPaymentAdjustmentResponseDto
            {
                Id = item.Id,
                AmountDelta = item.AmountDelta,
                PaidLessonCountDelta = item.PaidLessonCountDelta,
                Reason = item.Reason,
                RecordedAt = item.RecordedAt,
                RecordedByUserId = item.RecordedByUserId
            }).ToList()
        };
    }

    private static StudentPaymentAdjustmentResponseDto ToResponse(StudentPaymentAdjustmentEntity adjustment) => new()
    {
        Id = adjustment.Id,
        AmountDelta = adjustment.AmountDelta,
        PaidLessonCountDelta = adjustment.PaidLessonCountDelta,
        Reason = adjustment.Reason,
        RecordedAt = adjustment.RecordedAt,
        RecordedByUserId = adjustment.RecordedByUserId
    };
}
