using Dapper;
using DbSyncEngine.Application.Persistence;
using DbSyncEngine.Application.Persistence.Schema;
using DbSyncEngine.Application.Pipelines.Common;
using DbSyncEngine.Infrastructure.Persistence.Abstractions;
using DbSyncEngine.Infrastructure.Persistence.Common;

namespace DbSyncEngine.Infrastructure.Persistence.Schema.Readers;

public class PostgresSchemaReader : ISchemaReader
{
    private readonly IDbConnectionFactory _connections;

    public PostgresSchemaReader(IDbConnectionFactory connections)
    {
        _connections = connections;
    }

    public async Task<TableDefinition> ReadAsync(SyncContext ctx)
    {
        var source = ctx.Config.Source;
        using var conn = _connections.Create(source.Provider, source.ConnectionString);

        var table = source.Table;
        var schema = source.Schema ?? "public";

        var columns = await conn.QueryAsync<(string Name, string Type, bool IsNullable, int? Length, string? Default)>(
            @"
            SELECT column_name,
                   data_type,
                   is_nullable = 'YES' AS is_nullable,
                   character_maximum_length,
                   column_default
            FROM information_schema.columns
            WHERE table_schema = @schema AND table_name = @table;
        ", new { schema, table });

        return new TableDefinition
        {
            Name = table,
            PrimaryKey = ctx.Config.Key,
            Columns = columns.Select(c =>
            {
                var kind = IsPostgresIdentity(c.Type, c.Default)
                    ? ColumnKind.Identity
                    : ColumnKind.Normal;

                return new ColumnDefinition
                {
                    Name = c.Name,
                    Type = c.Type,
                    IsNullable = c.IsNullable,
                    Length = c.Length,
                    DefaultValue = c.Default,
                    Kind = kind
                };
            }).ToList()
        };
    }

    private static bool IsPostgresIdentity(string type, string? defaultValue)
    {
        if (!string.IsNullOrWhiteSpace(defaultValue) &&
            defaultValue.Contains("nextval(", StringComparison.OrdinalIgnoreCase))
            return true;

        var t = type.Trim().ToLowerInvariant();

        if (t is "serial" or "serial4" or "serial8" or "bigserial")
            return true;

        if (t.Contains("generated") && t.Contains("identity"))
            return true;

        return false;
    }
}