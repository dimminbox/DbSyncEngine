namespace DbSyncEngine.Application.Persistence.Abstracts;

public interface ISyncProcessRepositoryFactory
{
    ISyncProcessRepository Create();
}