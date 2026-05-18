using DbSyncEngine.Application.Persistence.Schema;
using DbSyncEngine.Application.Pipelines.Abstractions;
using DbSyncEngine.Application.Pipelines.Common;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace DbSyncEngine.Application.Pipelines.Steps.FullSyncSteps;

public class EnsureTargetSchemaStep : ISyncStep
{
    private readonly ISchemaBootstrapper _bootstrapper;
    private readonly ISchemaNormalizerFactory _normalizerFactory;
    private readonly EnsureSchemaOptions _options;
    private readonly ILogger<EnsureTargetSchemaStep> _logger;

    public EnsureTargetSchemaStep(
        ISchemaBootstrapper bootstrapper,
        ISchemaNormalizerFactory normalizerFactory,
        IOptions<EnsureSchemaOptions> options,
        ILogger<EnsureTargetSchemaStep> logger)
    {
        _bootstrapper = bootstrapper;
        _normalizerFactory = normalizerFactory;
        _options = options.Value;
        _logger = logger;
    }

    public async Task HandleAsync(SyncContext ctx, Func<Task> next)
    {
        _logger.LogInformation("Ensuring target schema for {Entity}", ctx.Config.Name);

        // В EnsureTargetSchemaStep.HandleAsync
        var sourceTable = await _bootstrapper.ReadSourceSchemaAsync(ctx, ctx.CancellationToken);
        var normalizerCtx = new NormalizerContext
        {
            TargetProvider = ctx.Config.Target.Provider,
            TargetSchema = ctx.Config.Target.Schema,
            Options = ctx.Config.NormalizerOptions
        };
        var normalizer = _normalizerFactory.Create(normalizerCtx);
        var normalized = normalizer.Normalize(sourceTable, normalizerCtx);
        ctx.Config.Target.Columns = normalized.Columns.Select(x => x.Name).ToList();
        await _bootstrapper.ApplyNormalizedTableAsync(ctx, normalized, ctx.CancellationToken);

        ctx.Config.Target.Key = ctx.Config.Source.Key;
        await next();
    }
}