using System;
using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;
using BookStore.Grains.Grains;
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

namespace BookStore.Host
{
    class Program
    {
        private static ILogger _log;
        private static ISiloHost silo;
        private static readonly TaskCompletionSource<bool> _wait = new TaskCompletionSource<bool>();

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
            
            var config = new ClusterConfiguration();
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
            
            silo = new SiloHostBuilder()
                .UseConfiguration(config)
                .ConfigureServices(s => s.AddLogging(b => b.AddSerilog()))
                .ConfigureApplicationParts(parts => parts.AddApplicationPart(typeof(PingGrain).Assembly).WithReferences())
                .Build();

            // AppDomain.CurrentDomain.ProcessExit += async (sender, eventArgs) => await Shutdown();
            Console.CancelKeyPress += async (sender, eventArgs) =>
            {
                await Shutdown();
                _wait.SetResult(true);
                eventArgs.Cancel = true;
            };

            await Startup();

            await _wait.Task;
        }

        private static async Task Startup()
        {   
            _log.Information("Starting silo");     
            await silo.StartAsync();
            _log.Information("Silo started");
        }

        private static async Task Shutdown()
        {
            _log.Information("Stopping silo");
            await silo.StopAsync();
            _log.Information("Silo stopped");
        }
    }
}
