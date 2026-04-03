namespace DbSyncEngine.Application.Persistence.Schema;

public class TableDefinition
{
    public string Name { get; set; } = default!;
    public string PrimaryKey { get; set; } = default!;
    public List<ColumnDefinition> Columns { get; set; } = new();
}

public class ColumnDefinition
{
    public string Name { get; set; } = default!;
    public string Type { get; set; } = default!;
    public bool IsNullable { get; set; }
    public int? Length { get; set; }
    public string? DefaultValue { get; set; }
    public ColumnKind Kind { get; set; } = ColumnKind.Normal;
}

public enum ColumnKind
{
    Normal,
    Identity
}