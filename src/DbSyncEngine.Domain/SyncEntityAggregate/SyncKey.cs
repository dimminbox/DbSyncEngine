using DbSyncEngine.Domain.SyncEntityAggregate.ValueObjects;
using DbSyncEngine.Domain.SyncProcessAggregate.Abstractions;

namespace DbSyncEngine.Domain.SyncEntityAggregate;

public record SyncKey(ColumnName Column, Type Type)
{
    public object Extract(IRowAccessor row)
    {
        var raw = row.GetRaw(Column.Value);

        if (raw is null)
            throw new InvalidOperationException($"Key column '{Column.Value}' cannot be null");

        return Convert.ChangeType(raw, Type);
    }

    public T Extract<T>(IRowAccessor row)
    {
        var raw = row.GetRaw(Column.Value);

        if (raw is null)
            throw new InvalidOperationException($"Key column '{Column.Value}' cannot be null");

        return (T)Convert.ChangeType(raw, typeof(T));
    }
}