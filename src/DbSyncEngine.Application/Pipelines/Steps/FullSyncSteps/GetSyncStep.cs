using DbSyncEngine.Application.Persistence;
using DbSyncEngine.Application.Pipelines.Abstractions;
using DbSyncEngine.Application.Pipelines.Common;
using DbSyncEngine.Domain.SyncProcessAggregate;
using Microsoft.Extensions.Logging;

namespace DbSyncEngine.Application.Pipelines.Steps.FullSyncSteps;

public class GetSyncStep : ISyncStep
{
    private readonly ISyncProcessRepositoryFactory _factory;
    private readonly ILogger<GetSyncStep> _logger;

    public GetSyncStep(
        ISyncProcessRepositoryFactory factory,
        ILogger<GetSyncStep> logger)
    {
        _factory = factory;
        _logger = logger;
    }

    public async Task HandleAsync(SyncContext ctx, Func<Task> next)
    {
        var config = ctx.Config;

        var repo = _factory.Create();
        repo.InitDb();
        
        var process = await repo.GetAsync(
            config.Name,
            config.Source.Provider,
            config.Target.Provider,
            ctx.Direction,
            ctx.CancellationToken);

        if (process is null)
        {
            process = SyncProcess.CreateNew(
                config.Name,
                config.Source.Provider,
                config.Target.Provider,
                ctx.Direction,
                config.Source.KeyType);

            await repo.SaveAsync(process, ctx.CancellationToken);

            _logger.LogInformation(
                "Created new sync process for {Entity} ({Source} → {Target})",
                config.Name, config.Source.Provider, config.Target.Provider);
        }
        else if (process.RestartRequested)
        {
            _logger.LogInformation(
                "Restart requested for {Entity}. Resetting state.",
                config.Name);
            process.Reset();
            await repo.SaveAsync(process, ctx.CancellationToken);
        }

        ctx.Process = process;

        await next();
    }
}