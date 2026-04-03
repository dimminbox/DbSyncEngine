using DbSyncEngine.Application.Persistence.Schema;
using DbSyncEngine.Application.Strategies.Options;

namespace DbSyncEngine.Infrastructure.Persistence.Schema.Normalization
{
    public class MySqlSchemaNormalizer : ISchemaNormalizer
    {
        public TableDefinition Normalize(TableDefinition sourceTable, NormalizerContext ctx)
        {
            if (sourceTable == null) throw new ArgumentNullException(nameof(sourceTable));
            if (ctx == null) throw new ArgumentNullException(nameof(ctx));

            var opts = ctx.Options ?? new NormalizerOptions();

            var tableName = ApplySchemaPrefix(sourceTable.Name, ctx.TargetSchema);
            tableName = ApplyCase(tableName, opts);

            var normalizedColumns = sourceTable.Columns.Select(c =>
            {
                var colName = ResolveColumnName(c.Name, opts);
                colName = ApplyCase(colName, opts);

                var mappedType = MapToMySqlType(c.Type, c.Length, c.Kind, opts);

                return new ColumnDefinition
                {
                    Name = colName,
                    Type = mappedType,
                    Length = c.Length,
                    IsNullable = c.Kind != ColumnKind.Identity && c.IsNullable,
                    DefaultValue = c.Kind == ColumnKind.Identity
                        ? null
                        : (opts.PreserveDefaults ? c.DefaultValue : null),
                    Kind = c.Kind
                };
            }).ToList();

            var pk = string.IsNullOrWhiteSpace(sourceTable.PrimaryKey)
                ? normalizedColumns.FirstOrDefault(c => c.Kind == ColumnKind.Identity)?.Name ?? string.Empty
                : ApplyCase(sourceTable.PrimaryKey, opts);

            return new TableDefinition
            {
                Name = tableName,
                PrimaryKey = pk,
                Columns = normalizedColumns
            };
        }

        private static string ResolveColumnName(string sourceName, NormalizerOptions opts)
        {
            if (opts?.ColumnRenameMap != null && opts.ColumnRenameMap.TryGetValue(sourceName, out var mapped))
                return mapped;
            return sourceName;
        }

        private static string ApplySchemaPrefix(string tableName, string? schema)
        {
            if (string.IsNullOrWhiteSpace(schema)) return tableName;
            return tableName.Contains('.') ? tableName : $"{schema}.{tableName}";
        }

        private static string ApplyCase(string name, NormalizerOptions opts)
        {
            if (opts == null) return name;
            return opts.PreserveCase ? name : name.ToLowerInvariant();
        }

        private static string MapToMySqlType(string sourceType, int? length, ColumnKind kind, NormalizerOptions opts)
        {
            if (string.IsNullOrWhiteSpace(sourceType)) return "TEXT";

            var t = sourceType.Trim().ToLowerInvariant();

            if (kind == ColumnKind.Identity)
                return "INT AUTO_INCREMENT";

            if (t.StartsWith("varchar"))
                return $"VARCHAR({length ?? opts.DefaultVarcharLength})";

            return t switch
            {
                "char" => length.HasValue ? $"CHAR({length.Value})" : "CHAR(1)",
                "text" => "TEXT",
                "int" or "integer" => "INT",
                "bigint" => "BIGINT",
                "smallint" => "SMALLINT",
                "boolean" or "bool" => "TINYINT(1)",
                "datetime" => "DATETIME",
                "timestamp" => "TIMESTAMP",
                "date" => "DATE",
                "decimal" or "numeric" => length.HasValue ? $"DECIMAL({length.Value})" : "DECIMAL(18,2)",
                "float" => "FLOAT",
                "double" => "DOUBLE",
                "json" => "JSON",
                _ => MapByHeuristics(t, length, opts)
            };
        }

        private static string MapByHeuristics(string t, int? length, NormalizerOptions opts)
        {
            if (t.StartsWith("varchar(") || t.StartsWith("char(") || t.StartsWith("decimal(") ||
                t.StartsWith("numeric("))
                return t.ToUpperInvariant();

            if (t.Contains("char")) return $"VARCHAR({length ?? opts.DefaultVarcharLength})";
            if (t.Contains("text")) return "TEXT";
            if (t.Contains("int")) return "INT";
            return t.ToUpperInvariant();
        }
    }
}