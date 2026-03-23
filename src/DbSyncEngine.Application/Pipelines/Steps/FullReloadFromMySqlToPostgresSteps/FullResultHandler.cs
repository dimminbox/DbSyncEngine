using DbSyncEngine.Application.Pipelines.Common;
using DbSyncEngine.Application.Pipelines.Steps.Common;

namespace DbSyncEngine.Application.Pipelines.Steps.FullReloadFromMySqlToPostgresSteps;

public class FullResultHandler<T> : IReadResultHandler<T> where T : class
{
    public Task HandleAsync(SyncContext ctx, IReadOnlyList<T> data)
    {
        List<object> list = new();

        foreach (var item in data)
        {
            list.Add(item);
        }

        ctx.SourceChunk = list;
        return Task.CompletedTask;
    }
}