using System;
using System.Net;
using System.Threading.Tasks;
using BookStore.Grains;
using Microsoft.Extensions.DependencyInjection;
using Orleans;
using Orleans.Hosting;
using Orleans.Runtime.Configuration;
using Serilog;
using Serilog.Events;
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
                .WriteTo.Console()
                .CreateLogger();
            
            _log = Log.ForContext<Program>();
            
            var config = new ClusterConfiguration();
            config.Globals.ClusterId = "orleans-docker";
            config.Globals.FastKillOnCancelKeyPress = true;
        
            // membership
            config.Globals.LivenessType = LivenessProviderType.Custom;
            config.Globals.DataConnectionString = "http://consul:8500";
            config.Globals.MembershipTableAssembly = "Orleans.Clustering.Consul";

            // reminders
            config.Globals.ReminderServiceType = ReminderServiceProviderType.Disabled;
            
            // IP and ports
            config.Defaults.Port = 11111;
            config.Defaults.ProxyGatewayEndpoint = new IPEndPoint(IPAddress.Any, 30000);

            silo = new SiloHostBuilder()
                .UseConfiguration(config)
                .ConfigureServices(s => s.AddLogging(b => b.AddSerilog()))
                .ConfigureApplicationParts(parts => parts.AddApplicationPart(typeof(PingGrain).Assembly).WithReferences())
                .Build();

            AppDomain.CurrentDomain.ProcessExit += (sender, eventArgs) => Shutdown();
            Console.CancelKeyPress += (sender, eventArgs) =>
            {
                Shutdown();
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

        private static void Shutdown()
        {
            _log.Information("Stopping silo");
            silo.Dispose();
            _log.Information("Silo stopped");
        }
    }
}
