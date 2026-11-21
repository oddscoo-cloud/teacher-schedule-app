namespace backend.Infrastructure.Services;

public static class LessonChargePolicy
{
    private static readonly TimeSpan FreeCancellationWindow = TimeSpan.FromHours(5);

    public static bool IsLateCancellation(DateTimeOffset startsAt, DateTimeOffset cancelledAt) =>
        cancelledAt <= startsAt && startsAt - cancelledAt < FreeCancellationWindow;
}
