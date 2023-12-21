using DevToys.Tools.Models;

namespace DevToys.Tools.Helpers;

internal static class DateHelper
{
    private static readonly DateTimeOffset minimumZoneOffsetTimestamp = DateTimeOffset.MinValue;

    private static readonly DateTimeOffset maximumZoneOffsetTimestamp = DateTimeOffset.MaxValue;

    internal static bool IsValidTimestamp(long value)
    {
        if (value < minimumZoneOffsetTimestamp.Millisecond || value > maximumZoneOffsetTimestamp.Millisecond)
            return false;
        return true;
    }

    internal static async Task<ToolResult<DateTimeOffset>> ConvertToDateTimeUtcAsync(
        long value,
        DateTimeOffset currentEpoch,
        DateFormat format,
        CancellationToken cancellationToken)
    {
        await TaskSchedulerAwaiter.SwitchOffMainThreadAsync(cancellationToken);

        DateTimeOffset conversionResult = format switch
        {
            DateFormat.Ticks => new DateTime(value, DateTimeKind.Utc),
            DateFormat.Seconds => currentEpoch.AddSeconds(value),
            DateFormat.Milliseconds => currentEpoch.AddMilliseconds(value),
            _ => throw new NotSupportedException(""),
        };

        cancellationToken.ThrowIfCancellationRequested();
        return new(conversionResult);
    }

    internal static async Task<ToolResult<long>> ConvertToLongAsync(
        DateTimeOffset value,
        DateTimeOffset epoch,
        DateFormat format,
        CancellationToken cancellationToken)
    {
        await TaskSchedulerAwaiter.SwitchOffMainThreadAsync(cancellationToken);

        DateTimeOffset dateTimeOffsetUtc = TimeZoneInfo.ConvertTime(value, TimeZoneInfo.Utc);
        TimeSpan elapsedTime = dateTimeOffsetUtc - epoch;
        long result = format switch
        {
            DateFormat.Ticks => dateTimeOffsetUtc.Ticks,
            DateFormat.Seconds => (long)elapsedTime.TotalSeconds,
            DateFormat.Milliseconds => (long)elapsedTime.TotalMilliseconds,
            _ => throw new NotSupportedException(""),
        };
        cancellationToken.ThrowIfCancellationRequested();
        return new(result);
    }

    internal static ToolResult<DateTimeOffset> ChangeDateTime(
        int value,
        DateTimeOffset target,
        TimeZoneInfo timeZoneInfo,
        DateValueType valueChanged)
    {
        target = TimeZoneInfo.ConvertTime(target, timeZoneInfo);
        DateTimeOffset result = valueChanged switch
        {
            DateValueType.Year => ChangeYear(target, value),
            DateValueType.Month => target.AddMonths(value - target.Month),
            DateValueType.Day => target.AddDays(value - target.Day),
            DateValueType.Hour => target.AddHours(value - target.Hour),
            DateValueType.Minute => target.AddMinutes(value - target.Minute),
            DateValueType.Second => target.AddSeconds(value - target.Second),
            DateValueType.Millisecond => target.AddMilliseconds(value - target.Millisecond),
            _ => throw new NotImplementedException(),
        };
        return new(result, true);
    }

    private static DateTimeOffset ChangeYear(DateTimeOffset target, int value)
    {
        if (value <= 0)
        {
            return DateTimeOffset.UtcNow;
        }

        if (!DateTime.IsLeapYear(value) && target.Month == 2 && target.Day > 28)
        {
            target.AddDays(value - target.Day);
        }
        return target.AddYears(value - target.Year);
    }
}
