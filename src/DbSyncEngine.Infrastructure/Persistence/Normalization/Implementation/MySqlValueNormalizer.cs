using DbSyncEngine.Application.Normalization;

namespace DbSyncEngine.Infrastructure.Persistence.Normalization.Implementation;

public class MySqlValueNormalizer : IValueNormalizer
{
    public object? Normalize(object? value)
    {
        if (value is null) return null;

        return value switch
        {
            sbyte v => (int)v,
            byte v => (int)v,
            short v => (int)v,
            long v => (long)v,
            decimal v => v,
            double v => (float)v,
            float v => v,
            DateTime dt => DateTime.SpecifyKind(dt, DateTimeKind.Utc),
            _ => value
        };
    }
}