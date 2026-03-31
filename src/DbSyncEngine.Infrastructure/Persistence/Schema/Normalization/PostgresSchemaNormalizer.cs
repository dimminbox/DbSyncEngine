using DbSyncEngine.Application.Persistence.Schema;
using DbSyncEngine.Application.Strategies.Options;

namespace DbSyncEngine.Infrastructure.Persistence.Schema.Normalization
{
    public class PostgresSchemaNormalizer : ISchemaNormalizer
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

                var mappedType = MapToPostgresType(c.Type, c.Length, opts);

                var defaultValue = opts.PreserveDefaults ? c.DefaultValue : null;

                return new ColumnDefinition
                {
                    Name = colName,
                    Type = mappedType,
                    IsNullable = c.IsNullable,
                    Length = c.Length,
                    DefaultValue = defaultValue
                };
            }).ToList();

            var pk = string.IsNullOrWhiteSpace(sourceTable.PrimaryKey) ? string.Empty : ApplyCase(sourceTable.PrimaryKey, opts);

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
            // For Postgres it's common to use lower_case unquoted identifiers
            return opts.PreserveCase ? name : name.ToLowerInvariant();
        }

        private static string MapToPostgresType(string sourceType, int? length, NormalizerOptions opts)
        {
            if (string.IsNullOrWhiteSpace(sourceType)) return "TEXT";

            var t = sourceType.Trim().ToLowerInvariant();

            if (t.StartsWith("varchar"))
                return $"VARCHAR({length ?? opts.DefaultVarcharLength})";

            return t switch
            {
                "char" => length.HasValue ? $"CHAR({length.Value})" : "CHAR(1)",
                "text" => "TEXT",
                "int" or "integer" => "INTEGER",
                "bigint" => "BIGINT",
                "smallint" => "SMALLINT",
                "boolean" or "bool" => "BOOLEAN",
                "datetime" => "TIMESTAMP",
                "timestamp" => "TIMESTAMP",
                "date" => "DATE",
                "decimal" or "numeric" => length.HasValue ? $"NUMERIC({length.Value})" : "NUMERIC(18,2)",
                "float" => "REAL",
                "double" => "DOUBLE PRECISION",
                "json" => "JSONB",
                _ => MapByHeuristics(t, length, opts)
            };
        }

        private static string MapByHeuristics(string t, int? length, NormalizerOptions opts)
        {
            if (t.StartsWith("varchar(") || t.StartsWith("char(") || t.StartsWith("numeric(") || t.StartsWith("decimal("))
                return t.ToUpperInvariant();

            if (t.Contains("char")) return $"VARCHAR({length ?? opts.DefaultVarcharLength})";
            if (t.Contains("text")) return "TEXT";
            if (t.Contains("int")) return "INTEGER";
            return t.ToUpperInvariant();
        }
    }
}
