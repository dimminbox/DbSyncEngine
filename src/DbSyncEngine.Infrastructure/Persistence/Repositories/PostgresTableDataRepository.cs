using System.Text;
using Dapper;
using DbSyncEngine.Application.Persistence;
using DbSyncEngine.Application.Pipelines.Common;
using Npgsql;

namespace DbSyncEngine.Infrastructure.Persistence.Repositories;

public class PostgresTableDataRepository : TableDataRepositoryBase, ITableDataRepository
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
        string? lastKey,
        string lastKeyType,
        int batchSize,
        CancellationToken ct)
    {
        // Экранируем имена колонок и таблиц
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

                        try
                        {
                            await WriteValueAsync(writer, col, value, ct);
                        }
                        catch (Exception exCol)
                        {
                            Console.WriteLine("❌ ERROR writing column:");
                            Console.WriteLine($"   Column: {col}");
                            Console.WriteLine($"   Value: {value}");
                            Console.WriteLine($"   Type: {value?.GetType()}");
                            Console.WriteLine($"   Error: {exCol.Message}");
                            throw;
                        }
                    }
                }
                catch (Exception exRow)
                {
                    Console.WriteLine("❌ COPY failed on row:");
                    foreach (var kv in row.Values)
                        Console.WriteLine($"   {kv.Key} = {kv.Value}");

                    Console.WriteLine($"   Error: {exRow.Message}");
                    throw;
                }
            }

            await writer.CompleteAsync(ct);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[ERROR] Insert batch failed. Error: {ex.Message}");
            throw;
        }
        finally
        {
            await _connection.CloseAsync();
        }
    }


    private async Task WriteValueAsync(
        NpgsqlBinaryImporter writer,
        string col,
        object? value,
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

            case string s:
                var utf8 = Encoding.UTF8.GetBytes(s);
                if (!IsValidUtf8(utf8))
                {
                    Console.WriteLine($"⚠ Non-UTF8 string in column '{col}': {BitConverter.ToString(utf8)}");
                    utf8 = Encoding.Convert(Encoding.GetEncoding("windows-1251"), Encoding.UTF8, utf8);
                    s = Encoding.UTF8.GetString(utf8);
                }

                await writer.WriteAsync(s, ct);
                break;

            case byte[] bytes:
                if (!IsValidUtf8(bytes))
                {
                    Console.WriteLine($"⚠ Non-UTF8 byte[] in column '{col}': {BitConverter.ToString(bytes)}");
                    bytes = Encoding.Convert(Encoding.GetEncoding("windows-1251"), Encoding.UTF8, bytes);
                }

                await writer.WriteAsync(bytes, ct);
                break;

            default:
                await writer.WriteAsync(value.ToString(), ct);
                break;
        }
    }

    private static bool IsValidUtf8(byte[] bytes)
    {
        try
        {
            Encoding.UTF8.GetString(bytes);
            return true;
        }
        catch
        {
            return false;
        }
    }
}