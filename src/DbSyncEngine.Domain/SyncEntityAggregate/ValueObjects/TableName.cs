namespace DbSyncEngine.Domain.SyncEntityAggregate.ValueObjects;

public record TableName(string Value)
{
    public override string ToString() => Value;
}