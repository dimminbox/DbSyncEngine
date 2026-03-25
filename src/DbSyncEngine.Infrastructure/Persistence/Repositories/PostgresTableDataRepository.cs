using Dapper;
using DbSyncEngine.Application.Persistence.Abstracts;
using DbSyncEngine.Application.Pipelines.Common;
using DbSyncEngine.Infrastructure.Persistence.Abstractions;
using Npgsql;

namespace DbSyncEngine.Infrastructure.Persistence.Repositories;

public class PostgresTableDataRepository : ITableDataRepository
{
    private readonly NpgsqlConnection _connection;

    public PostgresTableDataRepository(NpgsqlConnection connection)
    {
        _connection = connection;
    }

    public async Task<IReadOnlyList<RowData>> ReadChunkAsync(
        string tableName,
        IReadOnlyList<string> columns,
        string keyColumn,
        object? lastKey,
        int batchSize,
        CancellationToken ct)
    {
        // Экранируем имена колонок и таблиц
        var columnList = string.Join(",", columns.Select(c => $"\"{c}\""));

        var whereClause = lastKey != null
            ? $"WHERE \"{keyColumn}\" > @lastKey"
            : string.Empty;

        var sql = $@"
            SELECT {columnList}
            FROM ""{tableName}""
            {whereClause}
            ORDER BY ""{keyColumn}""
            LIMIT @batchSize";


        var rows = await _connection.QueryAsync<IDictionary<string, object?>>(
            new CommandDefinition(
                sql,
                new { lastKey, batchSize },
                cancellationToken: ct));

        return rows.Select(r => new RowData(r.AsReadOnly())).ToList();
    }

    public async Task WriteChunkAsync(
        string tableName,
        IReadOnlyList<string> columns,
        IReadOnlyList<RowData> rows,
        CancellationToken ct)
    {
        if (rows.Count == 0)
            return;

        var columnList = string.Join(",", columns.Select(c => $"\"{c}\""));

        // COPY BINARY — самый быстрый способ записи в PostgreSQL
        using var writer = await _connection.BeginBinaryImportAsync(
            $"COPY \"{tableName}\" ({columnList}) FROM STDIN (FORMAT BINARY)",
            ct);

        foreach (var row in rows)
        {
            await writer.StartRowAsync(ct);

            foreach (var col in columns)
            {
                var value = row.Values[col];

                // Npgsql сам определит тип и запишет корректно
                await writer.WriteAsync(value, ct);
            }
        }

        await writer.CompleteAsync(ct);
    }
}