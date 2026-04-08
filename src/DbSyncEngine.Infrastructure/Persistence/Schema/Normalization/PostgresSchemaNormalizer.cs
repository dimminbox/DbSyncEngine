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

                var mappedType = MapToPostgresType(c, opts);

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

            var pk = string.IsNullOrWhiteSpace(sourceTable.PrimaryKey)
                ? string.Empty
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
            // For Postgres it's common to use lower_case unquoted identifiers
            return opts.PreserveCase ? name : name.ToLowerInvariant();
        }

        private static string MapToPostgresType(ColumnDefinition c, NormalizerOptions opts)
        {
            var t = (c.Type ?? "text").Trim().ToLowerInvariant();

            // 1) AUTO_INCREMENT → SERIAL4
            if (c.Kind == ColumnKind.Identity)
                return "SERIAL4";

            // 2) GUID → uuid
            if ((t.StartsWith("char") || t.StartsWith("varchar")) && c.Length == 36)
                return "UUID";

            // 3) varchar
            if (t.StartsWith("varchar"))
                return $"VARCHAR({c.Length ?? opts.DefaultVarcharLength})";

            // 4) text
            if (t.Contains("text"))
                return "TEXT";

            // 5) decimal → numeric(28,10)
            if (t.StartsWith("decimal") || t.StartsWith("numeric"))
                return "NUMERIC(28,10)";

            // 6) double → float4
            if (t.StartsWith("double"))
                return "FLOAT4";

            // 7) float → float4
            if (t.StartsWith("float"))
                return "FLOAT4";

            // 8) int → int4
            if (t.StartsWith("int"))
                return "INT4";

            // 9) bigint
            if (t.StartsWith("bigint"))
                return "INT8";

            // 10) smallint
            if (t.StartsWith("smallint"))
                return "INT2";

            // 11) datetime / timestamp
            if (t.Contains("datetime") || t.Contains("timestamp"))
                return "TIMESTAMP";

            // 12) date
            if (t == "date")
                return "DATE";

            // 13) bool
            if (t == "bool" || t == "boolean")
                return "BOOLEAN";

            // 14) json
            if (t == "json")
                return "JSONB";

            return t.ToUpperInvariant();
        }

        private static string MapByHeuristics(string t, int? length, NormalizerOptions opts)
        {
            if (t.StartsWith("varchar(") || t.StartsWith("char(") || t.StartsWith("numeric(") ||
                t.StartsWith("decimal("))
                return t.ToUpperInvariant();

            if (t.Contains("char")) return $"VARCHAR({length ?? opts.DefaultVarcharLength})";
            if (t.Contains("text")) return "TEXT";
            if (t.Contains("int")) return "INTEGER";
            return t.ToUpperInvariant();
        }
    }
}