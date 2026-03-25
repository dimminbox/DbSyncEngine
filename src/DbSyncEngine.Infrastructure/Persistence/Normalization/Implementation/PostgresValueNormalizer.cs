using DbSyncEngine.Application.Normalization;

namespace DbSyncEngine.Infrastructure.Persistence.Normalization.Implementation;

public class PostgresValueNormalizer :  IValueNormalizer
{
    public object? Normalize(object? value) => value;
}