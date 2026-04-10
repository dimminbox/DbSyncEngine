namespace DbSyncEngine.Application.Persistence;

public interface ISyncProcessRepositoryFactory
{
    ISyncProcessRepository Create();
}