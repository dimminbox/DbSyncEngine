using DbSyncEngine.Application.Strategies.Options;

namespace DbSyncEngine.Application.Persistence.Schema;

public interface ISchemaNormalizer
{
    TableDefinition Normalize(TableDefinition sourceSchema, NormalizerContext ctx);
}

public class NormalizerContext
{
    public string TargetProvider { get; init; } = default!;
    public string? TargetSchema { get; init; }
    public NormalizerOptions? Options { get; init; }
}