using System.Data;

namespace DbSyncEngine.Infrastructure.Persistence.Abstractions;

public interface IDbConnectionFactory
{
    IDbConnection Create(string provider, string connectionString);
}