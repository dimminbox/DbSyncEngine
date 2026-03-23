using DbSyncEngine.Application.Pipelines.Common;

namespace DbSyncEngine.Application.Pipelines.Comparison;

public interface IAggregateComparator
{
    AggregateDiff Compare(SyncAggregate mysql, SyncAggregate postgres);
}