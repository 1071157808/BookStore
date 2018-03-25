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
using Orleans.Configuration;
using Orleans.EventSourcing.CustomStorage;
using Orleans.Hosting;
using Orleans.Runtime;
using Orleans.Runtime.Configuration;
using Orleans.Storage;
using Serilog;
using Serilog.Events;
using Serilog.Sinks.SystemConsole.Themes;
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
                .WriteTo.Async(c => c.File("./logs/log.log", rollingInterval: RollingInterval.Day))
                .CreateLogger();
            
            _log = Log.ForContext<Program>();
            
            // AppDomain.CurrentDomain.ProcessExit += async (sender, eventArgs) => await Shutdown();
            Console.CancelKeyPress += (sender, eventArgs) =>
            {
                Shutdown();
                _wait.Set();
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
            
            Log.CloseAndFlush();
        }

        // orleans silo host
        private static ISiloHost BuildSilo(string[] args, Action<IServiceCollection> configureServices)
        {
            configureServices = configureServices ?? (s => { });
            
            var clusterId = "orleans-docker";
            
            var siloPort = 11111;
            var gatewayPort = 30000;
            var siloAddress = IPAddress.Loopback;
            
            var config = new ClusterConfiguration();
            config.Globals.RegisterLogConsistencyProvider<LogConsistencyProvider>("CustomStorage");

            return new SiloHostBuilder()
                .UseConfiguration(config)
                .ConfigureServices(configureServices)
                .ConfigureLogging(o => o.AddSerilog())
                .Configure(o => o.ClusterId = clusterId)
                .ConfigureEndpoints(siloAddress, siloPort, gatewayPort)
                .ConfigureApplicationParts(parts => parts.AddApplicationPart(typeof(PingGrain).Assembly).WithReferences())
                
                .AddAdoNetGrainStorage("AdoNetStorage", o =>
                {
                    o.Invariant = "Npgsql";
                    o.UseJsonFormat = true;
                    o.ConnectionString = "Server=localhost;Port=5432;Database=bookstore_grains;User ID=postgres;Pooling=false;";
                })
                                
                .UseAdoNetClustering(o =>
                {
                    o.AdoInvariant = "Npgsql";
                    o.ConnectionString = "Server=localhost;Port=5432;Database=bookstore_membership;User ID=postgres;Pooling=false;";
                })                
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
            services.AddSingleton(_eventStoreConnection);
        }
    }
}
