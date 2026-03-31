using DbSyncEngine.Application.Persistence.Schema;

namespace DbSyncEngine.Infrastructure.Persistence.Schema.Ddl;

public class PostgresDdlGenerator : ITargetDdlGenerator
{
    public int CommandTimeout { get; } = 30;

    public string GenerateCreateTable(TableDefinition table, string? schema) =>
        GenerateCreateTableWithName(table, table.Name, schema);

    public string GenerateCreateTableWithName(TableDefinition table, string tableName, string? schema)
    {
        var full = Qualify(schema, tableName);
        var cols = string.Join(", ",
            table.Columns.Select(c => $"\"{Escape(c.Name)}\" {MapType(c)} {(c.IsNullable ? "" : "NOT NULL")}" +
                                      (HasDefault(c) ? $" DEFAULT {c.DefaultValue}" : "")));
        var pk = string.IsNullOrWhiteSpace(table.PrimaryKey) ? "" : $", PRIMARY KEY (\"{Escape(table.PrimaryKey)}\")";
        return $"CREATE TABLE IF NOT EXISTS {full} ({cols}{pk});";
    }

    public string GenerateDropTable(string tableName, string? schema) =>
        $"DROP TABLE IF EXISTS {Qualify(schema, tableName)};";

    public string GenerateTableExistsSql(string tableName, string? schema)
    {
        var sch = string.IsNullOrWhiteSpace(schema) ? "public" : EscapeLiteral(schema);
        return
            $"SELECT COUNT(*) FROM information_schema.tables WHERE table_schema = '{sch}' AND table_name = '{EscapeLiteral(tableName)}';";
    }

    public string GenerateTempTableName(string tableName) =>
        $"{tableName}__tmp_{Guid.NewGuid():N}";

    public string? GenerateCopyDataSql(string sourceTable, string tempTable, string? schema)
    {
        var s = Qualify(schema, sourceTable);
        var t = Qualify(schema, tempTable);
        // naive copy; assumes compatible columns
        return $"INSERT INTO {t} SELECT * FROM {s};";
    }

    public string GenerateSwapTableSql(string targetTable, string tempTable, string? schema)
    {
        // In Postgres we can perform transactional rename: rename old -> old_bak, temp -> old, then drop old_bak
        var s = string.IsNullOrEmpty(schema) ? "" : $"\"{Escape(schema)}\".";
        var oldBak = $"{targetTable}__old_{Guid.NewGuid():N}";
        var sb = new System.Text.StringBuilder();
        sb.AppendLine("BEGIN;");
        sb.AppendLine($"ALTER TABLE {s}\"{Escape(targetTable)}\" RENAME TO \"{Escape(oldBak)}\";");
        sb.AppendLine($"ALTER TABLE {s}\"{Escape(tempTable)}\" RENAME TO \"{Escape(targetTable)}\";");
        sb.AppendLine("COMMIT;");
        return sb.ToString();
    }

    public string? GenerateCleanupAfterSwapSql(string targetTable, string tempTable, string? schema)
    {
        var s = string.IsNullOrEmpty(schema) ? "" : $"\"{Escape(schema)}\".";
        // We don't know exact oldBak name here; recommend no-op and let ReplaceTableAsync handle cleanup if needed.
        return null;
    }

    // Helpers
    private static string Qualify(string? schema, string name) =>
        string.IsNullOrWhiteSpace(schema) ? $"\"{Escape(name)}\"" : $"\"{Escape(schema)}\".\"{Escape(name)}\"";

    private static string Escape(string id) => id.Replace("\"", "\"\"");

    private static string EscapeLiteral(string s) => s.Replace("'", "''");

    private static bool HasDefault(ColumnDefinition c) =>
        !string.IsNullOrWhiteSpace(c.DefaultValue);

    private static string MapType(ColumnDefinition c)
    {
        var t = (c.Type ?? "text").Trim().ToLowerInvariant();

        if (t.StartsWith("varchar"))
        {
            if (c.Length.HasValue) return $"VARCHAR({c.Length.Value})";
            return "VARCHAR(255)";
        }

        return t switch
        {
            "char" => c.Length.HasValue ? $"CHAR({c.Length.Value})" : "CHAR(1)",
            "text" => "TEXT",
            "int" or "integer" => "INTEGER",
            "bigint" => "BIGINT",
            "smallint" => "SMALLINT",
            "boolean" or "bool" => "BOOLEAN",
            "datetime" => "TIMESTAMP",
            "timestamp" => "TIMESTAMP",
            "date" => "DATE",
            "decimal" or "numeric" => c.Length.HasValue ? $"NUMERIC({c.Length.Value})" : "NUMERIC(18,2)",
            "float" => "REAL",
            "double" => "DOUBLE PRECISION",
            "json" => "JSONB",
            _ => MapByHeuristics(t, c.Length)
        };
    }

    private static string MapByHeuristics(string t, int? length)
    {
        if (t.Contains("char")) return length.HasValue ? $"VARCHAR({length.Value})" : "VARCHAR(255)";
        if (t.Contains("text")) return "TEXT";
        if (t.Contains("int")) return "INTEGER";
        return t.ToUpperInvariant();
    }
}