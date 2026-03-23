using MicroOrm.Dapper.Repositories.Config;
using MicroOrm.Dapper.Repositories.SqlGenerator;
using Orders.Core.Models.Entites;
using Relef.Repository.Models;

namespace DbSyncEngine.Application.Helper;

public class CustomSqlGenerator<T> : SqlGenerator<T> where T : Base
{
    public CustomSqlGenerator() : base()
    {
        if (MicroOrmConfig.SqlProvider == SqlProvider.MySQL)
        {
            if (typeof(T) == typeof(Order))
            {
                SqlProperties = SqlProperties
                    .Where(p => p.PropertyName != nameof(Order.Id))
                    .ToArray();

                KeySqlProperties = KeySqlProperties
                    .Where(p => p.PropertyName != nameof(Order.Id))
                    .ToArray();

                if (IdentitySqlProperty != null && IdentitySqlProperty.PropertyName == nameof(Order.Id))
                {
                    IdentitySqlProperty = null;
                }
            }

            TableName = MicroOrmConfig.UseQuotationMarks ? $"\"{TableName}\"" : TableName;
        }

        if (MicroOrmConfig.SqlProvider == SqlProvider.PostgreSQL)
        {
            foreach (var prop in SqlProperties)
            {
                prop.ColumnName = prop.ColumnName.ToLower();
            }

            foreach (var key in KeySqlProperties)
            {
                key.ColumnName = key.ColumnName.ToLower();
            }

            if (IdentitySqlProperty != null)
            {
                IdentitySqlProperty.ColumnName = IdentitySqlProperty.ColumnName.ToLower();
            }

            TableName = MicroOrmConfig.UseQuotationMarks ? $"{TableName.ToLowerInvariant()}" : TableName.ToLowerInvariant();
        }
    }
}