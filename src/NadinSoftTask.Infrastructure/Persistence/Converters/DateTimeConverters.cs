using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using NadinSoftTask.Infrastructure.Common;

namespace NadinSoftTask.Infrastructure.Persistence.Converters;

public static class DateTimeConverters
{
    public static readonly ValueConverter<DateTime, DateTime> UtcDateTime = new(
        value => Normalize(value),
        value => DateTime.SpecifyKind(value, DateTimeKind.Utc));

    public static readonly ValueConverter<DateTime?, DateTime?> NullableUtcDateTime = new(
        value => value.HasValue ? Normalize(value.Value) : null,
        value => value.HasValue ? DateTime.SpecifyKind(value.Value, DateTimeKind.Utc) : null);

    private static DateTime Normalize(DateTime value) =>
        UtcDateTimes.NormalizeToMinute(value);
}
