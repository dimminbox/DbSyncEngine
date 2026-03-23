namespace DbSyncEngine.Domain.SyncEntityAggregate.ValueObjects;

public record ColumnName(string Value)
{
    public override string ToString() => Value;
}