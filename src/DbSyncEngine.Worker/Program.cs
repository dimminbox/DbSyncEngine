using DbSyncEngine.Infrastructure.Persistence.Schema;
using Microsoft.Data.Sqlite;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace DbSyncEngine.Worker
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = Host.CreateApplicationBuilder(args);

            InitSyncDb(builder);
            Run(builder);
        }

        private static void Run(HostApplicationBuilder builder)
        {
            builder.Services.AddWorkerServices(builder.Configuration);
            builder.Services.AddHostedService<SyncBackgroundService>();
            var app = builder.Build();
            app.Run();
        }

        private static void InitSyncDb(HostApplicationBuilder builder)
        {
            var sqliteConnString = builder.Configuration.GetConnectionString("SyncProcessDb");
            using var conn = new SqliteConnection(sqliteConnString);
            conn.Open();
            SyncProcessSchemaInitializer.EnsureCreated(conn);
        }
    }
}