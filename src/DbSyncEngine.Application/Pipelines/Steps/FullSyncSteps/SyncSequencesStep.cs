using DbSyncEngine.Application.Persistence.Schema;
using DbSyncEngine.Application.Pipelines.Abstractions;
using DbSyncEngine.Application.Pipelines.Common;
using Microsoft.Extensions.Logging;

namespace DbSyncEngine.Application.Pipelines.Steps.FullSyncSteps;

public class SyncSequencesStep : ISyncStep
{
    private readonly ISequenceSynchronizer _synchronizer;
    private readonly ILogger<SyncSequencesStep> _logger;

    public SyncSequencesStep(
        ISequenceSynchronizer synchronizer,
        ILogger<SyncSequencesStep> logger)
    {
        _synchronizer = synchronizer;
        _logger = logger;
    }

    public async Task HandleAsync(SyncContext ctx, Func<Task> next)
    {
        var key = ctx.Config.Target.Key;
        if (string.IsNullOrWhiteSpace(key))
        {
            _logger.LogWarning("No key column configured for {Table}, skipping sequence sync", ctx.Config.Target.Table);
            await next();
            return;
        }

        await _synchronizer.SyncAsync(ctx, key, ctx.CancellationToken);
        await next();
    }
}
