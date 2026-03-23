using System.Data;
using Dapper;

namespace DbSyncEngine.Infrastructure.Persistence.Common;

public class DapperRepository<T>
{
    protected readonly IDbConnection Connection;

    protected DapperRepository(IDbConnection connection)
    {
        Connection = connection;
    }

    protected Task<T?> QuerySingleAsync(string sql, object? param = null)
        => Connection.QuerySingleOrDefaultAsync<T>(sql, param);

    protected Task<IEnumerable<T>> QueryAsync(string sql, object? param = null)
        => Connection.QueryAsync<T>(sql, param);

    protected Task<int> ExecuteAsync(string sql, object? param = null)
        => Connection.ExecuteAsync(sql, param);
}