namespace DbSyncEngine.Application.Persistence;

public interface ITableDataRepositoryFactory
{
    ITableDataRepository Create(string provider, string connectionString);
}