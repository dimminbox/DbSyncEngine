using DbSyncEngine.Application.Pipelines.Abstractions;
using DbSyncEngine.Application.Strategies.Options;
using Microsoft.Extensions.Options;
using DbSyncEngine.Domain.SyncProcessAggregate.Enums;

namespace DbSyncEngine.Application.Pipelines.Common;

public class SyncPipeline : ISyncPipeline
{
    private readonly IReadOnlyList<ISyncStep> _steps;
    private readonly SyncEntityConfig _config;

    public SyncPipeline(IEnumerable<ISyncStep> steps, SyncEntityConfig config)
    {
        _steps = steps.ToList();
        _config = config;
    }

    public Task RunAsync(SyncDirection direction, CancellationToken ct)
    {
        var context = new SyncContext(_config)
        {
            Direction = direction, Now = DateTimeOffset.UtcNow, CancellationToken = ct
        };
        return InvokeStepAsync(0, context, ct);
    }

    private Task InvokeStepAsync(int index, SyncContext context, CancellationToken ct)
    {
        if (index >= _steps.Count) return Task.CompletedTask;
        return _steps[index].HandleAsync(context, () => InvokeStepAsync(index + 1, context, ct));
    }
}