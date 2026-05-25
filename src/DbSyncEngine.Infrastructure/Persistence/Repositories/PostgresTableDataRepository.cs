using System.Data;
using System.Text;
using Dapper;
using DbSyncEngine.Application.Persistence;
using DbSyncEngine.Application.Pipelines.Common;
using DbSyncEngine.Infrastructure.Persistence.Exceptions;
using Microsoft.Extensions.Logging;
using Npgsql;

namespace DbSyncEngine.Infrastructure.Persistence.Repositories;

public class PostgresTableDataRepository : TableDataRepositoryBase, ITableDataRepository
{
    private readonly NpgsqlConnection _connection;
    private readonly ILogger<PostgresTableDataRepository> _logger;
    private readonly string _schema;

    public PostgresTableDataRepository(
        NpgsqlConnection connection,
        ILogger<PostgresTableDataRepository> logger,
        string? schema = null)
    {
        _connection = connection;
        _logger = logger;
        _schema = string.IsNullOrWhiteSpace(schema) ? "public" : schema;
    }

    public async Task<IReadOnlyList<RowData>> ReadChunkAsync(
        string tableName,
        IReadOnlyList<string> columns,
        string keyColumn,
        string? lastKey,
        string lastKeyType,
        int batchSize,
        CancellationToken ct)
    {
        var columnList = columns.Any() ? string.Join(",", columns.Select(c => $"\"{c}\"")) : "*";

        var lastKeyTyped = ConvertKey(lastKey, lastKeyType);
        var whereClause = BuildWhereClause(keyColumn, lastKeyTyped);

        var sql = $@"
            SELECT {columnList}
            FROM ""{tableName}""
            {whereClause}
            ORDER BY ""{keyColumn}""
            LIMIT @batchSize";

        var rows = await _connection.QueryAsync<dynamic>(
            new CommandDefinition(
                sql,
                new { lastKeyTyped, batchSize },
                cancellationToken: ct));

        return rows.Select(r => new RowData((IDictionary<string, object?>)r)).ToList();
    }

    public async Task WriteChunkAsync(
        string tableName,
        IReadOnlyList<string> columns,
        IReadOnlyList<RowData> rows,
        int chunkSize,
        CancellationToken ct)
    {
        if (rows.Count == 0)
            return;

        var columnList = string.Join(",", columns.Select(c => $"\"{c}\""));

        try
        {
            await _connection.OpenAsync(ct);
            if (_connection.State != ConnectionState.Open)
                throw new ConnectionException("Cannot open connection");

            var columnTypes = await GetColumnTypesAsync(tableName, ct);

            using var writer = await _connection.BeginBinaryImportAsync(
                $"COPY \"{tableName}\" ({columnList}) FROM STDIN (FORMAT BINARY)",
                ct);

            foreach (var row in rows)
            {
                try
                {
                    await writer.StartRowAsync(ct);

                    foreach (var col in columns)
                    {
                        var value = row.Values[col];
                        columnTypes.TryGetValue(col, out var pgType);

                        try
                        {
                            await WriteValueAsync(writer, col, value, pgType, ct);
                        }
                        catch (Exception exCol)
                        {
                            _logger.LogError(exCol,
                                "Error writing column {Column} in table {Table} (value={Value}, clrType={ClrType})",
                                col, tableName, value, value?.GetType().Name);
                            throw;
                        }
                    }
                }
                catch (Exception exRow) when (exRow is not OperationCanceledException)
                {
                    var rowDump = string.Join(", ", row.Values.Select(kv => $"{kv.Key}={kv.Value}"));
                    _logger.LogError(exRow, "COPY failed on row in table {Table}: [{Row}]", tableName, rowDump);
                    throw;
                }
            }

            await writer.CompleteAsync(ct);
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            _logger.LogError(ex, "COPY batch failed for table {Table}", tableName);
            throw;
        }
        finally
        {
            await _connection.CloseAsync();
        }
    }

    private async Task<Dictionary<string, string>> GetColumnTypesAsync(string tableName, CancellationToken ct)
    {
        const string sql = """
            SELECT column_name, data_type
            FROM information_schema.columns
            WHERE table_schema = @schema
              AND table_name   = @tableName
            """;
        var rows = await _connection.QueryAsync<(string column_name, string data_type)>(
            new CommandDefinition(sql, new { schema = _schema, tableName }, cancellationToken: ct));
        return rows.ToDictionary(r => r.column_name, r => r.data_type, StringComparer.OrdinalIgnoreCase);
    }

    private async Task WriteValueAsync(
        NpgsqlBinaryImporter writer,
        string col,
        object? value,
        string? pgType,
        CancellationToken ct)
    {
        if (value == null)
        {
            await writer.WriteNullAsync(ct);
            return;
        }

        switch (value)
        {
            case Guid g:
                await writer.WriteAsync(g, ct);
                break;

            case DateTime dt:
                await writer.WriteAsync(dt, ct);
                break;

            case int i:
                await writer.WriteAsync(i, ct);
                break;

            case decimal dec:
                await writer.WriteAsync(dec, ct);
                break;

            case float ft:
                await writer.WriteAsync(ft, ct);
                break;

            case bool b:
                await writer.WriteAsync(b, ct);
                break;

            case string s when pgType == "uuid":
                if (!Guid.TryParse(s, out var guid))
                    throw new InvalidOperationException(
                        $"Cannot write value '{s}' to uuid column '{col}': not a valid UUID");
                await writer.WriteAsync(guid, ct);
                break;

            case string s:
                var utf8 = Encoding.UTF8.GetBytes(s);
                if (!System.Text.Unicode.Utf8.IsValid(utf8))
                {
                    _logger.LogWarning(
                        "Non-UTF8 string detected in column {Column}, attempting windows-1251 conversion",
                        col);
                    utf8 = Encoding.Convert(Encoding.GetEncoding("windows-1251"), Encoding.UTF8, utf8);
                    s = Encoding.UTF8.GetString(utf8);
                }

                await writer.WriteAsync(s, ct);
                break;

            case byte[] bytes:
                if (!System.Text.Unicode.Utf8.IsValid(bytes))
                {
                    _logger.LogWarning(
                        "Non-UTF8 byte[] detected in column {Column}, attempting windows-1251 conversion",
                        col);
                    bytes = Encoding.Convert(Encoding.GetEncoding("windows-1251"), Encoding.UTF8, bytes);
                }

                await writer.WriteAsync(bytes, ct);
                break;

            default:
                await writer.WriteAsync(value.ToString(), ct);
                break;
        }
    }
}
