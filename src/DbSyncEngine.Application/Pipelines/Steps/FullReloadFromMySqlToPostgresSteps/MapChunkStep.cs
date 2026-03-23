using DbSyncEngine.Application.Pipelines.Abstractions;
using DbSyncEngine.Application.Pipelines.Common;
using DbSyncEngine.Application.Pipelines.Mappers.Abstractions;
using Microsoft.Extensions.Logging;

namespace DbSyncEngine.Application.Pipelines.Steps.FullReloadFromMySqlToPostgresSteps;

public class MapChunkStep<TSource, TTarget> : ISyncStep
{
    private readonly IEntityMapper<TSource, TTarget> _mapper;
    private readonly ILogger<MapChunkStep<TSource, TTarget>> _logger;

    public MapChunkStep(IEntityMapper<TSource, TTarget> mapper, ILogger<MapChunkStep<TSource, TTarget>> logger)
    {
        _mapper = mapper;
        _logger = logger;
    }

    public Task HandleAsync(SyncContext ctx, Func<Task> next)
    {
        if (ctx.SourceChunk == null || ctx.SourceChunk.Count == 0)
        {
            ctx.TargetChunk = Array.Empty<object>();
            return next();
        }

        _logger.LogInformation("Mapping {Count} rows from {Source} to {Target}", ctx.SourceChunk.Count,
            typeof(TSource).Name, typeof(TTarget).Name);

        var mapped = new List<object>(ctx.SourceChunk.Count);
        foreach (var item in ctx.SourceChunk)
        {
            var source = (TSource)item;
            var target = _mapper.Map(source);
            mapped.Add(target);
        }
        
        ctx.TargetChunk = mapped;
        return next();
    }
}