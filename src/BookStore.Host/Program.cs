using System;
using System.Collections.Generic;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using BookStore.Grains.Grains;
using BookStore.Host.EventStore;
using EventStore.ClientAPI;
using EventStore.ClientAPI.SystemData;
using Microsoft.Extensions.DependencyInjection;
using Orleans;
using Orleans.EventSourcing.CustomStorage;
using Orleans.Hosting;
using Orleans.Runtime.Configuration;
using Orleans.Storage;
using Serilog;
using Serilog.Events;
using Serilog.Sinks.SystemConsole.Themes;
using static Orleans.Runtime.Configuration.GlobalConfiguration;
using ILogger = Serilog.ILogger;

namespace BookStore.Host
{
    class Program
    {
        private static ILogger _log;
        private static ISiloHost _silo;
        private static IEventStoreConnection _eventStoreConnection;
        private static readonly ManualResetEventSlim _wait = new ManualResetEventSlim(false);

        static async Task Main(string[] args)
        {
            Log.Logger = new LoggerConfiguration()
                .MinimumLevel.Information()
                .MinimumLevel.Override("Orleans", LogEventLevel.Warning)
                .MinimumLevel.Override("Runtime", LogEventLevel.Warning)
                .Enrich.FromLogContext()
                .WriteTo.Console(theme: AnsiConsoleTheme.Code)
                .CreateLogger();
            
            _log = Log.ForContext<Program>();
            
            // AppDomain.CurrentDomain.ProcessExit += async (sender, eventArgs) => await Shutdown();
            Console.CancelKeyPress += (sender, eventArgs) =>
            {
                Shutdown();
                _wait.Set();
                eventArgs.Cancel = true;
            };

            await Startup(args);

            _wait.Wait();
        }

        private static async Task Startup(string[] args)
        {
            await StartEventStoreConnection(args);
            await StartSilo(args);
        }

        private static void Shutdown()
        {
            StopEventStoreConnection();
            StopSilo();
        }

        // orleans silo host
        private static ISiloHost BuildSilo(string[] args, Action<IServiceCollection> configureServices)
        {
            configureServices = configureServices ?? (s => { });
            
            var config = new ClusterConfiguration();
            config.Globals.FastKillOnCancelKeyPress = false;
            
            config.Globals.ClusterId = "orleans-docker";
            config.Globals.FastKillOnCancelKeyPress = true;
        
            // membership
            config.Globals.AdoInvariant = "Npgsql";
            config.Globals.LivenessType = LivenessProviderType.SqlServer;
            config.Globals.DataConnectionString = "Server=localhost;Port=5432;Database=bookstore_membership;User ID=postgres;Pooling=false;";

            // reminders
            config.Globals.ReminderServiceType = ReminderServiceProviderType.Disabled;
            
            // IP and ports
            config.Defaults.Port = 11111;
            config.Defaults.ProxyGatewayEndpoint = new IPEndPoint(IPAddress.Any, 30000);

            // storage
            config.Globals.RegisterStorageProvider<AdoNetStorageProvider>("AdoNetStorage",
                new Dictionary<string, string>
                {
                    {"AdoInvariant", "Npgsql"},
                    {"UseJsonFormat", "true"},
                    {
                        "DataConnectionString",
                        "Server=localhost;Port=5432;Database=bookstore_grains;User ID=postgres;Pooling=false;"
                    },
                });
            config.Globals.RegisterLogConsistencyProvider<LogConsistencyProvider>("CustomStorage");
            
            return new SiloHostBuilder()
                .UseConfiguration(config)
                .ConfigureServices(configureServices)
                .ConfigureApplicationParts(parts => parts.AddApplicationPart(typeof(PingGrain).Assembly).WithReferences())
                .Build();
        }

        private static async Task StartSilo(string[] args)
        {
            _log.Debug("Building silo host");
            _silo = BuildSilo(args, ConfigureServices);

            _log.Information("Starting orleans silo host");
            await _silo.StartAsync();
            _log.Information("Orleans silo host started");
        }

        private static void StopSilo()
        {
            _log.Information("Stopping orleans silo host");
            _silo.StopAsync().GetAwaiter().GetResult();
            _silo.Dispose();
            _log.Information("Orleans silo host stopped");
        }

        // event store connection
        private static IEventStoreConnection BuildEventStoreConnection(string[] args)
        {
            var settings = ConnectionSettings.Create()
                .SetDefaultUserCredentials(new UserCredentials("admin", "admin"))
                .UseCustomLogger(new EventStoreLogger());
            
            return  EventStoreConnection.Create(settings, new IPEndPoint(IPAddress.Loopback, 1113));
        }

        private static async Task StartEventStoreConnection(string[] args)
        {
            _log.Debug("Building event store connection");
            _eventStoreConnection = BuildEventStoreConnection(args);
            
            _log.Information("Starting event store connection");
            await _eventStoreConnection.ConnectAsync();
            _log.Information("Event store connection started");
        }

        private static void StopEventStoreConnection()
        {
            _log.Information("Stopping event store connection");
            _eventStoreConnection.Close();
            _eventStoreConnection.Dispose();
            _log.Information("Event store connection stopped");
        }
        
        private static void ConfigureServices(IServiceCollection services)
        {
            services.AddLogging(b => b.AddSerilog());
            services.AddSingleton(_eventStoreConnection);
        }
    }
}
