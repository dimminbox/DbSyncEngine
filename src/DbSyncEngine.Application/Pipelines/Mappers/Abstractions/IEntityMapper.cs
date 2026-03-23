namespace DbSyncEngine.Application.Pipelines.Mappers.Abstractions;

public interface IEntityMapper<TSource, TTarget>
{
    TTarget Map(TSource source);
}