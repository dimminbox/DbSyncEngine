using Microsoft.Extensions.Hosting;

namespace DbSyncEngine.Application
{
    public class Program
    {
        public static void Main(string[] args) { CreateHostBuilder(args).Build().Run(); }

        public static IHostBuilder CreateHostBuilder(string[] args) => Host.CreateDefaultBuilder(args)
            .ConfigureServices((hostContext, services) =>
            {
                var startup = new Startup();
                startup.ConfigureServices(services);
            });
    }
}