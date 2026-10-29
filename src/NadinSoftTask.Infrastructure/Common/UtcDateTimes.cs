namespace NadinSoftTask.Infrastructure.Common;

internal static class UtcDateTimes
{
    public static DateTime NormalizeToMinute(DateTime value)
    {
        DateTime utc = value.Kind switch
        {
            DateTimeKind.Utc => value,
            DateTimeKind.Local => value.ToUniversalTime(),
            _ => DateTime.SpecifyKind(value, DateTimeKind.Utc)
        };

        return new DateTime(
            utc.Ticks - (utc.Ticks % TimeSpan.TicksPerMinute),
            DateTimeKind.Utc);
    }
}
