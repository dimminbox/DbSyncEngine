using DbSyncEngine.Domain.SyncProcessAggregate.Abstractions;

namespace DbSyncEngine.Application.Pipelines.Common;

public class RowData : IRowAccessor
{
    private readonly Dictionary<string, object?> _values;
    public IReadOnlyDictionary<string, object?> Values => _values;
    
    public RowData()
    {
        _values = new Dictionary<string, object?>(StringComparer.OrdinalIgnoreCase);
    }

    public RowData(IDictionary<string, object?> values)
    {
        _values = new Dictionary<string, object?>(values, StringComparer.OrdinalIgnoreCase);
    }
    
    public object? GetRaw(string column)
        => _values.TryGetValue(column, out var v) ? v : null;
    
    public T Get<T>(string column)
    {
        var raw = GetRaw(column);
        if (raw is T t)
            return t;

        return (T)Convert.ChangeType(raw!, typeof(T));
    }
    
    public void Set(string column, object? value)
        => _values[column] = value;
    
    public bool TryGetValue(string column, out object? value)
        => _values.TryGetValue(column, out value);
}