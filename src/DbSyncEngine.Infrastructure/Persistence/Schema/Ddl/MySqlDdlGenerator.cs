using DbSyncEngine.Application.Persistence.Schema;

namespace DbSyncEngine.Infrastructure.Persistence.Schema.Ddl;

public class MySqlDdlGenerator : ITargetDdlGenerator
{
    public int CommandTimeout { get; } = 30;

    public string GenerateCreateTable(TableDefinition table, string? schema) =>
        GenerateCreateTableWithName(table, table.Name, schema);

    public string GenerateCreateTableWithName(TableDefinition table, string tableName, string? schema)
    {
        var full = Qualify(schema, tableName);
        var cols = string.Join(", ",
            table.Columns.Select(c => $"`{Escape(c.Name)}` {MapType(c)} {(c.IsNullable ? "NULL" : "NOT NULL")}" +
                                      (HasDefault(c) ? $" DEFAULT {c.DefaultValue}" : "")));
        var pk = string.IsNullOrWhiteSpace(table.PrimaryKey) ? "" : $", PRIMARY KEY (`{Escape(table.PrimaryKey)}`)";
        return $"CREATE TABLE IF NOT EXISTS {full} ({cols}{pk}) ENGINE=InnoDB;";
    }

    public string GenerateDropTable(string tableName, string? schema) =>
        $"DROP TABLE IF EXISTS {Qualify(schema, tableName)};";

    public string GenerateTableExistsSql(string tableName, string? schema)
    {
        var s = schema ?? "DATABASE()";
        if (string.IsNullOrWhiteSpace(schema))
        {
            return $"SELECT COUNT(*) FROM information_schema.tables " +
                   $"WHERE table_schema = DATABASE() AND table_name = '{EscapeLiteral(tableName)}';";
        }
        
        return $"SELECT COUNT(*) FROM information_schema.tables " +
               $"WHERE table_schema = '{EscapeLiteral(schema)}' AND table_name = '{EscapeLiteral(tableName)}';";
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
        var s = string.IsNullOrEmpty(schema) ? "" : $"`{Escape(schema)}`.";
        // atomic rename: old -> old__bak, temp -> old
        var oldBak = $"{targetTable}__old_{Guid.NewGuid():N}";
        return
            $"RENAME TABLE {s}`{Escape(targetTable)}` TO {s}`{Escape(oldBak)}`, {s}`{Escape(tempTable)}` TO {s}`{Escape(targetTable)}`;";
    }

    public string? GenerateCleanupAfterSwapSql(string targetTable, string tempTable, string? schema)
    {
        // cleanup any old backups by pattern (best-effort). Here we drop the specific old name is not known to caller,
        // so return empty and let bootstrapper handle explicit drop if needed.
        return null;
    }

    // Helpers

    private static string Qualify(string? schema, string name) =>
        string.IsNullOrWhiteSpace(schema) ? $"`{Escape(name)}`" : $"`{Escape(schema)}`.`{Escape(name)}`";

    private static string Escape(string id) => id.Replace("`", "``");

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
            "uuid" => "CHAR(36)",
            "serial" or "serial4" or "int" or "integer" or "int4" => "INT",
            "bigserial" or "serial8" or "bigint" or "int8" => "BIGINT",
            "smallint" => "SMALLINT",
            "char" => c.Length.HasValue ? $"CHAR({c.Length.Value})" : "CHAR(1)",
            "text" => "TEXT",
            "boolean" or "bool" => "TINYINT(1)",
            "datetime" => "DATETIME",
            "timestamp" => "TIMESTAMP",
            "date" => "DATE",
            "decimal" or "numeric" => c.Length.HasValue ? $"DECIMAL({c.Length.Value})" : "DECIMAL(18,2)",
            "float" or "float4" => "FLOAT",
            "double" => "DOUBLE",
            "json" => "JSON",
            _ when t.StartsWith("varchar") => $"VARCHAR({c.Length ?? 255})",
            _ => MapByHeuristics(t, c.Length)
        };
    }

    private static string MapByHeuristics(string t, int? length)
    {
        if (t.StartsWith("varchar(") || t.StartsWith("char(") || t.StartsWith("decimal(") || t.StartsWith("numeric("))
            return t.ToUpperInvariant();
        if (t.Contains("char")) return $"VARCHAR({length ?? 255})";
        if (t.Contains("text")) return "TEXT";

        return t.ToUpperInvariant();
    }
}