using DbSyncEngine.Application.Helper;
using DbSyncEngine.Application.Strategies.Abstractions;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using Polly;

namespace DbSyncEngine.Application;

public class SyncBackgroundService : BackgroundService
{
    private readonly ISyncStrategyFactory _factory;
    private readonly IOptionsMonitor<SyncOptions> _options;

    public SyncBackgroundService(ISyncStrategyFactory factory, IOptionsMonitor<SyncOptions> options)
    {
        _factory = factory;
        _options = options;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var retryPolicy = Policy
            .Handle<Exception>()
            .WaitAndRetryAsync(
                retryCount: _options.CurrentValue.MaxInsertRetries,
                sleepDurationProvider: attempt =>
                    TimeSpan.FromSeconds(Math.Pow(2, attempt)),
                onRetry: (exception, delay, attempt, context) =>
                {
                    Console.WriteLine(
                        $"Pipeline failed on attempt {attempt}. Retrying in {delay}. Error: {exception.Message}, stacktrace {exception.StackTrace}");
                });

        while (!stoppingToken.IsCancellationRequested)
        {
            var direction = _options.CurrentValue.Direction;
            var strategy = _factory.Create(direction);

            try
            {
                await retryPolicy.ExecuteAsync(
                    ct => strategy.RunAsync(ct),
                    stoppingToken);
            }
            catch (Exception ex)
            {
                Console.WriteLine(
                    $"Pipeline failed after all retries. Stopping service. Error: {ex.Message}");
                return; // <-- сервис завершится
            }

            await Task.Delay(_options.CurrentValue.Interval, stoppingToken);
        }
    }
}