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

    protected async Task<T?> QuerySingleAsync(string sql, object? param = null)
    {
        try
        {
            if (Connection.State != ConnectionState.Open)
                Connection.Open();
            return await Connection.QuerySingleOrDefaultAsync<T>(sql, param);
        }
        finally
        {
            Connection.Close();
        }
    }

    protected async Task<int> ExecuteAsync(string sql, object? param = null)
    {
        try
        {
            if (Connection.State != ConnectionState.Open)
                Connection.Open();
            return await Connection.ExecuteAsync(sql, param);
        }
        finally
        {
            Connection.Close();
        }
    }
}