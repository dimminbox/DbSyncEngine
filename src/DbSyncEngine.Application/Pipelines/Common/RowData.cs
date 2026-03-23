using DbSyncEngine.Domain.SyncProcessAggregate.Abstractions;

namespace DbSyncEngine.Application.Pipelines.Common;

public class RowData : IRowAccessor
{
    public IReadOnlyDictionary<string, object?> Values { get; }

    public RowData(IReadOnlyDictionary<string, object?> values)
    {
        Values = values;
    }

    public object? GetRaw(string column)
        => Values.TryGetValue(column, out var v) ? v : null;

    public T Get<T>(string column)
    {
        var raw = GetRaw(column);
        if (raw is T t)
            return t;

        return (T)Convert.ChangeType(raw!, typeof(T));
    }
}