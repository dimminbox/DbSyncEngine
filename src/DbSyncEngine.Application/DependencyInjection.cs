using DbSyncEngine.Application.Pipelines.Steps.FullSyncSteps;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace DbSyncEngine.Application;

public static class DependencyInjection
{
   
    public static IServiceCollection AddApplication(this IServiceCollection services,
        ConfigurationManager configuration)
    {
        services
            .AddSteps();
        return services;
    }
    

    private static IServiceCollection AddSteps(this IServiceCollection services)
    {
        services.AddTransient<ReadDataStep>();
        services.AddTransient<MapChunkStep>();
        services.AddTransient<GetSyncStep>();
        services.AddTransient<UpdateSyncStep>();
        services.AddTransient<WriteDataStep>(); 
        return services;
    }
}