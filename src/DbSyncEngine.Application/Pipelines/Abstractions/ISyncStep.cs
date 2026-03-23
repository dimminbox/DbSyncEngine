using DbSyncEngine.Application.Pipelines.Common;

namespace DbSyncEngine.Application.Pipelines.Abstractions;

public interface ISyncStep
{
    Task HandleAsync(SyncContext context, Func<Task> next);
}