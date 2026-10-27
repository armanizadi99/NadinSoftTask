using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace NadinSoftTask.Infrastructure.Persistence.Converters;

public static class DateTimeConverters
{
    public static readonly ValueConverter<DateTime, DateTime> UtcDateTime = new(
        value => Normalize(value),
        value => DateTime.SpecifyKind(value, DateTimeKind.Utc));

    public static readonly ValueConverter<DateTime?, DateTime?> NullableUtcDateTime = new(
        value => value.HasValue ? Normalize(value.Value) : null,
        value => value.HasValue ? DateTime.SpecifyKind(value.Value, DateTimeKind.Utc) : null);

    private static DateTime Normalize(DateTime value)
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
