using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace NadinSoftTask.Infrastructure.Persistence.Converters;

public static class UserIdValueConverter
{
    public static readonly ValueConverter<string, Guid> Instance = new(
        value => Guid.Parse(value),
        value => value.ToString());
}
