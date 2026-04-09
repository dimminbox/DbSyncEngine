using DbSyncEngine.Application;
using DbSyncEngine.Application.Strategies.Options;
using DbSyncEngine.Infrastructure;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace DbSyncEngine.Worker;

public static class DependencyInjection
{
   
    public static IServiceCollection AddWorkerServices(
        this IServiceCollection services,
        ConfigurationManager configuration)
    {
        services
            .AddConfiguration(configuration)
            .AddApplication(configuration)
            .AddInfrastructure(configuration);

        return services;
    }
    

    private static IServiceCollection AddConfiguration(this IServiceCollection services,
        ConfigurationManager configuration)
    {
        configuration
            .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
            .AddJsonFile("appsettings.Development.json", optional: true, reloadOnChange: true)
            .AddUserSecrets<Program>()
            .AddEnvironmentVariables();
        services.BindConfigurations(configuration);

        return services;
    }

    private static IServiceCollection BindConfigurations(this IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<SyncConfig>(configuration.GetSection("Sync"));
        return services;
    }
}