namespace backend.Presentation.DTOs;

using System.ComponentModel.DataAnnotations;

public sealed class SetStudentPriceRequestDto
{
    [Range(typeof(decimal), "0", "9999999999999999.99")]
    public decimal Amount { get; set; }
}

public sealed class RecordStudentPaymentRequestDto
{
    [Range(typeof(decimal), "0.01", "9999999999999999.99")]
    public decimal Amount { get; set; }

    [Range(0, int.MaxValue)]
    public int PaidLessonCount { get; set; }

    public DateTimeOffset? PaidAt { get; set; }

    [Required, StringLength(100, MinimumLength = 1)]
    public string IdempotencyKey { get; set; } = null!;
}

public sealed class AdjustStudentPaymentRequestDto
{
    [Range(typeof(decimal), "-9999999999999999.99", "9999999999999999.99")]
    public decimal AmountDelta { get; set; }

    public int PaidLessonCountDelta { get; set; }

    [Required, StringLength(500, MinimumLength = 1)]
    public string Reason { get; set; } = null!;

    [Required, StringLength(100, MinimumLength = 1)]
    public string IdempotencyKey { get; set; } = null!;
}

public sealed class StudentFinanceResponseDto
{
    public string Currency { get; set; } = "UAH";
    public decimal? StandardPrice { get; set; }
    public int PaidLessonCount { get; set; }
    public int ChargedLessonCount { get; set; }
    public int LessonBalance { get; set; }
    public decimal PaymentsTotal { get; set; }
    public decimal ChargedAmount { get; set; }
    public decimal MoneyBalance { get; set; }
    public bool LessonChargesIntegrated { get; set; }
    public List<StudentPriceHistoryResponseDto> PriceHistory { get; set; } = new();
    public List<StudentPaymentResponseDto> Payments { get; set; } = new();
    public List<StudentLessonChargeResponseDto> Charges { get; set; } = new();
}

public sealed class StudentLessonChargeResponseDto
{
    public int Id { get; set; }
    public int SourceLessonId { get; set; }
    public string Type { get; set; } = null!;
    public DateTimeOffset LessonStartsAt { get; set; }
    public DateTimeOffset ChargedAt { get; set; }
    public decimal Amount { get; set; }
    public int RecordedByUserId { get; set; }
}

public sealed class StudentPriceHistoryResponseDto
{
    public int Id { get; set; }
    public decimal Amount { get; set; }
    public DateTimeOffset EffectiveFrom { get; set; }
    public DateTimeOffset RecordedAt { get; set; }
    public int RecordedByUserId { get; set; }
}

public sealed class StudentPaymentResponseDto
{
    public int Id { get; set; }
    public decimal Amount { get; set; }
    public int PaidLessonCount { get; set; }
    public DateTimeOffset PaidAt { get; set; }
    public DateTimeOffset RecordedAt { get; set; }
    public int RecordedByUserId { get; set; }
    public decimal NetAmount { get; set; }
    public int NetPaidLessonCount { get; set; }
    public List<StudentPaymentAdjustmentResponseDto> Adjustments { get; set; } = new();
}

public sealed class StudentPaymentAdjustmentResponseDto
{
    public int Id { get; set; }
    public decimal AmountDelta { get; set; }
    public int PaidLessonCountDelta { get; set; }
    public string Reason { get; set; } = null!;
    public DateTimeOffset RecordedAt { get; set; }
    public int RecordedByUserId { get; set; }
}
