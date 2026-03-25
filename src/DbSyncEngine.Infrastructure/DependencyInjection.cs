using DbSyncEngine.Application.Normalization;
using DbSyncEngine.Application.Persistence.Abstracts;
using DbSyncEngine.Infrastructure.Persistence.Abstractions;
using DbSyncEngine.Infrastructure.Persistence.Fabrics;
using DbSyncEngine.Infrastructure.Persistence.Normalization.Implementation;
using DbSyncEngine.Infrastructure.Persistence.Repositories;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace DbSyncEngine.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services,
        ConfigurationManager configuration)
    {
        services
            .AddDataRepositories()
            .AddSyncRepositories()
            .AddNormalization();
        return services;
    }


    private static IServiceCollection AddSyncRepositories(this IServiceCollection services)
    {
        services.AddTransient<IDbConnectionFactory, DbConnectionFactory>();
        services.AddTransient<ISyncProcessRepositoryFactory, SyncProcessRepositoryFactory>();
        services.AddTransient<ISyncProcessRepository, SyncProcessRepository>();
        return services;
    }

    private static IServiceCollection AddDataRepositories(this IServiceCollection services)
    {
        services.AddTransient<ITableDataRepository, MySqlTableDataRepository>();
        services.AddTransient<ITableDataRepository, PostgresTableDataRepository>();
        services.AddTransient<ITableDataRepositoryFactory, TableDataRepositoryFactory>();
        services.AddTransient<IDbConnectionFactory, DbConnectionFactory>();
        return services;
    }

    private static IServiceCollection AddNormalization(this IServiceCollection services)
    {
        services.AddTransient<IValueNormalizer, MySqlValueNormalizer>();
        services.AddTransient<IValueNormalizer, PostgresValueNormalizer>();
        services.AddTransient<IValueNormalizerFactory, ValueNormalizerFactory>();
        return services;
    }
}