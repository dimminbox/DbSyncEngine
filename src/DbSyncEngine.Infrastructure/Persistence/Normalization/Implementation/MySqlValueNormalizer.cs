using DbSyncEngine.Application.Normalization;

namespace DbSyncEngine.Infrastructure.Persistence.Normalization.Implementation;

public class MySqlValueNormalizer : IValueNormalizer
{
    public object? Normalize(object? value)
    {
        if (value is null) return null;

        return value switch
        {
            sbyte v when v == 0 || v == 1 => v == 1,
            byte v when v == 0 || v == 1 => v == 1,
            short v when v == 0 || v == 1 => v == 1,
            
            sbyte v => (int)v,
            byte v => (int)v,
            short v => (int)v,
            int v => v,
            long v => (int)v,
            
            decimal v => v,
            
            double v => (float)v,
            float v => v,
            
            DateTime dt => DateTime.SpecifyKind(dt, DateTimeKind.Utc),
            
            _ => value
        };
    }
}