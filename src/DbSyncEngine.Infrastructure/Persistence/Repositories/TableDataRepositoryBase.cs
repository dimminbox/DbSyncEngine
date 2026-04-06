using System.Globalization;

namespace DbSyncEngine.Infrastructure.Persistence.Repositories;

public abstract class TableDataRepositoryBase
{
    protected object? ConvertKey(string? keyString, string keyType)
    {
        if (keyString == null)
            return null;

        if (keyType == "long")
            return long.Parse(keyString);
        
        if (keyType == "datetime")
            return DateTime.Parse(keyString, null, DateTimeStyles.RoundtripKind);

        return keyString;
    }

    protected string BuildWhereClause(string keyColumn, object? lastKeyTyped)
    {
        if (lastKeyTyped == null)
            return string.Empty;

        return $"WHERE {keyColumn} > @lastKeyTyped";
    }
}