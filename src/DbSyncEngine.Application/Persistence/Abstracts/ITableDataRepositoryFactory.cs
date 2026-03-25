using DbSyncEngine.Application.Persistence.Abstracts;

public interface ITableDataRepositoryFactory
{
    ITableDataRepository Create(string provider, string connectionString);
}