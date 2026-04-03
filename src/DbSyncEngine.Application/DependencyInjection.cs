using DbSyncEngine.Application.Pipelines.Steps.FullSyncSteps;
using DbSyncEngine.Application.Strategies.Abstractions;
using DbSyncEngine.Application.Strategies.Implementations;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace DbSyncEngine.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services,
        ConfigurationManager configuration)
    {
        services
            .AddStrategies()
            .AddSteps();
        return services;
    }


    private static IServiceCollection AddSteps(this IServiceCollection services)
    {
        services.AddTransient<EnsureTargetSchemaStep>();
        services.AddTransient<ReadDataStep>();
        services.AddTransient<MapChunkStep>();
        services.AddTransient<GetSyncStep>();
        services.AddTransient<UpdateSyncStep>();
        services.AddTransient<WriteDataStep>();
        return services;
    }

    private static IServiceCollection AddStrategies(this IServiceCollection services)
    {
        services.AddSingleton<ISyncStrategyFactory, SyncStrategyFactory>();
        return services;
    }
}