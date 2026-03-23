using DbSyncEngine.Application.Pipelines.Common;

namespace DbSyncEngine.Application.Pipelines.Comparison;

public interface IAggregateDecisionService
{
    SyncOperation Decide(SyncAggregate mysql, SyncAggregate postgres);
}