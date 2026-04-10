using Dapper;
using DbSyncEngine.Application.Persistence;
using DbSyncEngine.Application.Persistence.Schema;
using DbSyncEngine.Application.Pipelines.Common;
using DbSyncEngine.Infrastructure.Persistence.Abstractions;
using DbSyncEngine.Infrastructure.Persistence.Common;

namespace DbSyncEngine.Infrastructure.Persistence.Schema.Readers;

public class MySqlSchemaReader : ISchemaReader
{
    private readonly IDbConnectionFactory _connections;

    public MySqlSchemaReader(IDbConnectionFactory connections)
    {
        _connections = connections;
    }

    public async Task<TableDefinition> ReadAsync(SyncContext ctx)
    {
        var source = ctx.Config.Source;
        using var conn = _connections.Create(source.Provider, source.ConnectionString);

        var table = source.Table;
        var schema = source.Schema ?? conn.Database;

        var sql = @"SELECT
            c.COLUMN_NAME,
            c.DATA_TYPE,
            c.IS_NULLABLE,
            c.CHARACTER_MAXIMUM_LENGTH,
            c.COLUMN_DEFAULT,
            (c.COLUMN_KEY = 'PRI') AS is_primary_key
        FROM information_schema.columns c
        WHERE c.table_schema = @schema
          AND c.table_name = @table
        ORDER BY c.ORDINAL_POSITION;
        ";

        var columns =
            await conn
                .QueryAsync<(string Name, string Type, string IsNullable, int? Length, string? Default, bool
                    isPrimaryKey)>(sql, new { schema, table });

        if (!columns.Any())
        {
            throw new InvalidOperationException(
                $"Table '{schema}.{table}' does not exist in schema '{schema}' or has no columns.");
        }

        return new TableDefinition
        {
            Name = table,
            PrimaryKey = ctx.Config.Source.Key,
            Columns = columns.Select(c => new ColumnDefinition
            {
                Name = c.Name,
                Type = c.Type,
                IsNullable = c.IsNullable == "YES",
                Length = c.Length,
                DefaultValue = c.Default,
                Kind = c.isPrimaryKey ? ColumnKind.Identity : ColumnKind.Normal
            }).ToList()
        };
    }
}