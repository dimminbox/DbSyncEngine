namespace DbSyncEngine.Application.Persistence.Schema;

public interface ISchemaNormalizerFactory
{
    ISchemaNormalizer Create(NormalizerContext ctx);
}