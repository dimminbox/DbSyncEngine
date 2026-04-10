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
        await _bootstrapper.ApplyNormalizedTableAsync(ctx, normalized, ctx.CancellationToken);

        await next();
    }

    private async Task ProcessTableAsync(SyncContext ctx, TableDefinition table,
        IReadOnlyList<TableDefinition> targetSchema)
    {
        var exists = targetSchema.Any(t => t.Name.Equals(table.Name, StringComparison.OrdinalIgnoreCase));

        _logger.LogInformation("Table {Table} exists: {Exists}", table.Name, exists);

        if (_options.DryRun)
        {
            _logger.LogInformation("DryRun enabled. Planned action for {Table}: {Action}", table.Name,
                exists ? (_options.ReplaceIfExists ? "Replace" : "Skip") : "Create");
            return;
        }

        if (!exists)
        {
            await RetryAsync(() => _bootstrapper.CreateTableAsync(ctx, table, ctx.CancellationToken), ctx);
            _logger.LogInformation("Created table {Table}", table.Name);
            return;
        }

        if (_options.ReplaceIfExists)
        {
            // безопасная замена через bootstrapper
            await RetryAsync(() => _bootstrapper.ReplaceTableAsync(ctx, table, ctx.CancellationToken), ctx);
            _logger.LogInformation("Replaced table {Table}", table.Name);
            return;
        }

        _logger.LogInformation("Skipping existing table {Table}", table.Name);
    }

    private async Task RetryAsync(Func<Task> action, SyncContext ctx)
    {
        var attempts = 0;
        while (true)
        {
            try
            {
                await action();
                return;
            }
            catch (Exception ex) when (attempts < _options.RetryCount)
            {
                attempts++;
                _logger.LogWarning(ex, "Attempt {Attempt} failed for entity {Entity}. Retrying in {Delay}s", attempts,
                    ctx.Config.Name, _options.RetryDelay.TotalSeconds);
                await Task.Delay(_options.RetryDelay, ctx.CancellationToken);
            }
        }
    }
}