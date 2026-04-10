namespace DbSyncEngine.Domain.SyncProcessAggregate.Abstractions;

public interface IRowAccessor
{
    object? GetRaw(string column);
    bool TryGetValue(string column, out object? value);

    void Set(string column, object? value);
}