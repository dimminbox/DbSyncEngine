using DbSyncEngine.Application.Normalization;
using DbSyncEngine.Application.Persistence;
using DbSyncEngine.Application.Persistence.Schema;
using DbSyncEngine.Application.Strategies.Abstractions;
using DbSyncEngine.Application.Strategies.Implementations;
using DbSyncEngine.Infrastructure.Persistence.Abstractions;
using DbSyncEngine.Infrastructure.Persistence.Fabrics;
using DbSyncEngine.Infrastructure.Persistence.Normalization.Implementation;
using DbSyncEngine.Infrastructure.Persistence.Repositories;
using DbSyncEngine.Infrastructure.Persistence.Schema;
using DbSyncEngine.Infrastructure.Persistence.Schema.Ddl;
using DbSyncEngine.Infrastructure.Persistence.Schema.Normalization;
using DbSyncEngine.Infrastructure.Persistence.Schema.Readers;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace DbSyncEngine.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services,
        ConfigurationManager configuration)
    {
        services
            .AddSchema()
            .AddTableData()
            .AddSyncRepositories()
            .AddNormalization();
        return services;
    }


    private static IServiceCollection AddSyncRepositories(this IServiceCollection services)
    {
        services.AddTransient<IDbConnectionFactory, DbConnectionFactory>();
        services.AddTransient<ISyncProcessRepositoryFactory, SyncProcessRepositoryFactory>();
        return services;
    }

    private static IServiceCollection AddSchema(this IServiceCollection services)
    {
        services.AddSingleton<MySqlSchemaReader>();
        services.AddSingleton<PostgresSchemaReader>();
        services.AddSingleton<ISchemaReaderFactory, SchemaReaderFactory>();
        services.AddSingleton<ISchemaBootstrapper, SchemaBootstrapper>();
        services.AddSingleton<ISchemaNormalizerFactory, SchemaNormalizerFactory>();
        services.AddSingleton<ITargetDdlGeneratorFactory, TargetDdlGeneratorFactory>();
        return services;
    }

    private static IServiceCollection AddTableData(this IServiceCollection services)
    {
        services.AddTransient<ITableDataRepository, MySqlTableDataRepository>();
        services.AddTransient<ITableDataRepository, PostgresTableDataRepository>();
        services.AddTransient<ITableDataRepositoryFactory, TableDataRepositoryFactory>();
        services.AddTransient<IDbConnectionFactory, DbConnectionFactory>();
        return services;
    }

    private static IServiceCollection AddNormalization(this IServiceCollection services)
    {
        services.AddSingleton<IValueNormalizer, MySqlValueNormalizer>();
        services.AddSingleton<IValueNormalizer, PostgresValueNormalizer>();
        services.AddSingleton<IValueNormalizerFactory, ValueNormalizerFactory>();

        services.AddSingleton<MySqlSchemaNormalizer>();
        services.AddSingleton<PostgresSchemaNormalizer>();
        return services;
    }
}