using System.Reflection;
using DbSyncEngine.Application.Entities;
using DbSyncEngine.Application.Helper;
using DbSyncEngine.Application.Pipelines.Comparison;
using DbSyncEngine.Application.Pipelines.Mappers.Abstractions;
using DbSyncEngine.Application.Pipelines.Mappers.Implements;
using DbSyncEngine.Application.Pipelines.Steps.Common;
using DbSyncEngine.Application.Pipelines.Steps.Configs;
using DbSyncEngine.Application.Pipelines.Steps.FullReloadFromMySqlToPostgresSteps;
using DbSyncEngine.Application.Providers.Abstractions;
using DbSyncEngine.Application.Providers.Implementations;
using DbSyncEngine.Application.Strategies.Abstractions;
using DbSyncEngine.Application.Strategies.Implementations;
using DbSyncEngine.Application.Strategies.Options;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace DbSyncEngine.Application
{
    /// <summary>
    /// 
    /// </summary>
    [System.Diagnostics.CodeAnalysis.SuppressMessage(
        "Usage",
        "CA1515:Consider making public types internal",
        Justification = "Workers must be public for routing to work correctly")]
    public class Startup
    {
        const string ConsulServiceName = "Orders";
        private IConfiguration Configuration { get; }
        private static string ConsulConnectionString;
        private Setup Setup;

        /// <summary>
        /// 
        /// </summary>
        protected LogLevel FileLogLevel
        {
            get
            {
                var key = "Orders.Exchange:Log:FileLogLevel";
                var lvl = LogLevel.Information;

                if (Configuration != null && Configuration[key] != null)
                {
                    Enum.TryParse(Configuration[key], true, out lvl);
                }

                return lvl;
            }
        }


        /// <summary>
        /// 
        /// </summary>
        public Startup()
        {
            var environmentName = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT");
            var appDir = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            ConsulConnectionString = Environment.GetEnvironmentVariable("ConsulConnectionString");
            var config = new ConfigurationBuilder();
            if (!String.IsNullOrEmpty(ConsulConnectionString))
            {
                Setup = new Setup(new Uri(ConsulConnectionString), ConsulServiceName);

                var builder = Setup.Configuration();
                if (builder is ConfigurationBuilder)
                {
                    config.AddConfiguration(builder.Build());
                }
            }

            config
                .AddJsonFile(Path.Combine(appDir, "appsettings.json"), optional: true)
                .AddJsonFile($"appsettings.json", optional: true)
                .AddJsonFile($"appsettings.{environmentName}.json", optional: true)
                .AddEnvironmentVariables();

            Configuration = config.Build();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="services"></param>
        public void ConfigureServices(IServiceCollection services)
        {
            MicroOrmConfig.TablePrefix = "";
            MicroOrmConfig.AllowKeyAsIdentity = true;

            services.AddTransient<IMySqlProvider>(sp =>
            {
                var mysqlServices = new ServiceCollection();
                mysqlServices.AddSingleton<ISqlGeneratorFactory, SqlGeneratorFactory>();
                mysqlServices.AddTransient(_ => ConnectionFactory.GetConnection(Configuration, ConnectionEngine.MySqL));
                mysqlServices.AddTransient(typeof(IRepository<>), typeof(RepositoryWithCustomSqlGenerator<>));
                mysqlServices.AddSingleton(typeof(ISqlGenerator<>), typeof(SqlGenerator<>));
                mysqlServices.AddLogging(cfg => cfg.AddConsole());
                var mysqlProvider = mysqlServices.BuildServiceProvider();
                return new MySqlProvider(mysqlProvider);
            });

            services.AddTransient<IPostgresProvider>(sp =>
            {
                var postgresServices = new ServiceCollection();
                postgresServices.AddTransient(_ =>
                    ConnectionFactory.GetConnection(Configuration, ConnectionEngine.Postgres));
                postgresServices.AddSingleton<ISqlGeneratorFactory, SqlGeneratorFactory>();
                postgresServices.AddSingleton(typeof(ISqlGenerator<>), typeof(SqlGenerator<>));
                postgresServices.AddTransient(typeof(IRepository<>), typeof(RepositoryWithCustomSqlGenerator<>));
                postgresServices.AddTransient<IRepository<ReloadState>, Repository<ReloadState>>();
                postgresServices.AddLogging(cfg => cfg.AddConsole());
                var postgresProvider = postgresServices.BuildServiceProvider();
                return new PostgresProvider(postgresProvider);
            });

            services.AddSingleton(new ReloadEntityConfig<Order>
            {
                KeySelector = o => o.DateUpdate,
                KeyValueExtractor = o => o.DateUpdate,
                Comparison = (current, last) => ((DateTime)current) > ((DateTime)last)
            });

            services.AddSingleton(new ReloadEntityConfig<Product>
            {
                KeySelector = o => o.Id,
                KeyValueExtractor = o => o.Id,
                Comparison = (current, last) => ((DateTime)current) > ((DateTime)last)
            });

            services.AddTransient<IncrimentalMySqlToPostgresSyncStrategy>();
            services.AddTransient<FullReloadMySqlToPostgresSyncStrategy>();
            services.AddSingleton<ISyncStrategyFactory, SyncStrategyFactory>();

            services.AddSingleton(new ReloadEntityConfig<Order>
            {
                KeySelector = o => o.DateUpdate,
                KeyValueExtractor = o => o.DateUpdate,
                ParseKey = s => DateTime.Parse(s),
                Comparison = (current, last) => ((DateTime)current) > ((DateTime)last),
                InitialFilter = c => c.DateUpdate > DateTime.MinValue
            });
            services.AddSingleton(new ReloadEntityConfig<Product>
            {
                KeySelector = p => p.Id,
                KeyValueExtractor = p => p.Id,
                ParseKey = s => long.Parse(s),
                Comparison = (current, last) => ((long)current) > ((long)last),
                InitialFilter = c => c.Id > 0
            });

            services.AddTransient<IEntityMapper<Order, Order>, OrderToOrderPgMapper>();
            services.AddTransient<IEntityMapper<Product, Product>, ProductToProductPgMapper>();

            services.AddTransient(typeof(MapChunkStep<,>));
            services.AddTransient<PrepareReloadStep>();
            services.AddTransient(typeof(ReadDataStep));
            services.AddTransient<IReadResultHandler<Product>, FullResultHandler<Product>>();
            services.AddTransient(typeof(UpdateFullReloadStateStep<>));
            services.AddTransient(typeof(WriteChunkToPostgresStep<>));

            services.AddTransient<IAggregateComparator, AggregateComparator>();
            services.AddTransient<IAggregateDecisionService, AggregateDecisionService>();
            
            services.AddTransient<IReadResultHandler<Order>, IncrementalResultHandler>();
            
            services.AddTransient<CompareAggregatesStep>();
            services.AddTransient<EnrichMySqlAggregatesStep>();
            services.AddTransient<EnrichPostgresAggregatesStep>();
            services.AddTransient<TransformMySqlToPostgresStep>();
            services.AddTransient<PersistToPostgresStep>();
            services.AddTransient<UpdateIncrimentalReloadStateStep>();

            services.Configure<SyncOptions>(Configuration.GetSection("Sync"));
            services.Configure<FullReloadOptions>(Configuration.GetSection("FullReload"));
            services.AddHostedService<SyncBackgroundService>();
        }
    }
}