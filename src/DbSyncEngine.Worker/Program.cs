using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace DbSyncEngine.Worker
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = Host.CreateApplicationBuilder(args);
            builder.Services.AddWorkerServices(builder.Configuration);
            builder.Services.AddHostedService<SyncBackgroundService>();
            var app = builder.Build();
            app.Run();
        }
    }
}