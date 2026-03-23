using DbSyncEngine.Domain.SyncEntityAggregate.Exceptions;
using DbSyncEngine.Domain.SyncEntityAggregate.ValueObjects;

namespace DbSyncEngine.Domain.SyncEntityAggregate;

public class SyncEntityDescriptor
{
    public TableName SourceTable { get; }
    public TableName TargetTable { get; }
    public IReadOnlyList<ColumnName> Columns { get; }
    public IReadOnlyList<SyncKey> Keys { get; }

    public SyncEntityDescriptor(
        TableName sourceTable,
        TableName targetTable,
        IEnumerable<ColumnName> columns,
        IEnumerable<SyncKey> keys)
    {
        if (!columns.Any())
            throw new InvalidDescriptorException("Columns list cannot be empty");

        if (!keys.Any())
            throw new InvalidDescriptorException("Keys list cannot be empty");

        SourceTable = sourceTable;
        TargetTable = targetTable;
        Columns = columns.ToList();
        Keys = keys.ToList();
    }
}