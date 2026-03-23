namespace DbSyncEngine.Domain.SyncProcessAggregate.Abstractions;

public interface IRowAccessor
{
    object? GetRaw(string column);
    T Get<T>(string column);
}