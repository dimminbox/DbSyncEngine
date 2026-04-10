using System.Globalization;
using System.Text.RegularExpressions;

namespace DbSyncEngine.Infrastructure.Persistence.Repositories;

public abstract class TableDataRepositoryBase
{
    protected object? ConvertKey(string? keyString, string keyType)
    {
        if (string.IsNullOrEmpty(keyString))
            return null;

        if (keyType is null)
        {
            throw new ArgumentNullException(nameof(keyType));
        }

        return keyType.ToLowerInvariant() switch
        {
            "long" => long.Parse(keyString.ToString(CultureInfo.InvariantCulture)),
            "int" => int.Parse(keyString.ToString(CultureInfo.InvariantCulture)),
            "datetime" => DateTime.Parse(keyString.ToString(CultureInfo.InvariantCulture), null,
                DateTimeStyles.RoundtripKind),
            _ => throw new ArgumentException($"Invalid key type: {keyType}")
        };
    }

    protected string BuildWhereClause(string keyColumn, object? lastKeyTyped)
    {
        if (lastKeyTyped == null)
            return string.Empty;

        return $"WHERE {keyColumn} > @lastKeyTyped";
    }
}