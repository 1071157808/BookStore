using System;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using BookStore.Client.EventStore;
using BookStore.Contracts.Grains;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Hosting.Server.Features;
using Microsoft.Extensions.DependencyInjection;
using Orleans;
using Orleans.Hosting;
using Serilog;
using Serilog.Events;
using Serilog.Sinks.SystemConsole.Themes;

namespace BookStore.Client
{
    public class Program
    {
        private static ILogger _log;
        private static IWebHost _webHost;
        private static IClusterClient _orleansClient;
        private static readonly ManualResetEventSlim _wait = new ManualResetEventSlim(false);

        public static async Task Main(string[] args)
        {
            Log.Logger = new LoggerConfiguration()
                .MinimumLevel.Debug()
                .MinimumLevel.Override("Microsoft", LogEventLevel.Warning)
                .Enrich.FromLogContext()
                .WriteTo.Console(theme: AnsiConsoleTheme.Code)
                .CreateLogger();

            _log = Log.ForContext<Program>();            

            // AppDomain.CurrentDomain.ProcessExit += (sender, eventArgs) => Shutdown();
            Console.CancelKeyPress += (sender, eventArgs) =>
            {
                Shutdown();
                _wait.Set();
                eventArgs.Cancel = true;
            };

            await Startup(args);

            _wait.Wait();
        }

        private static IClusterClient BuildOrleansClient(string[] args)
        {
            var clusterId = "orleans-docker";

            return new ClientBuilder()
                .ConfigureCluster(options => options.ClusterId = clusterId)
                .ConfigureApplicationParts(parts => parts.AddApplicationPart(typeof(IPingGrain).Assembly).WithReferences())
                
                .UseAdoNetClustering(o =>
                {
                    o.AdoInvariant = "Npgsql";
                    o.ConnectionString = "Server=localhost;Port=5432;Database=bookstore_membership;User ID=postgres;Pooling=false;";
                })
                .Build();
        }

        private static IWebHost BuildWebHost(string[] args, Action<IServiceCollection> configureServices = null)
        {
            configureServices = configureServices ?? new Action<IServiceCollection>(s => { });

            return WebHost.CreateDefaultBuilder(args)
               .ConfigureServices(configureServices)
               .UseStartup<Startup>()
               .UseSerilog()
               .Build();
        }

        private static async Task Startup(string[] args)
        {
            await StartOrleansClient(args);
            await StartWebHost(args);
        }

        private static void Shutdown()
        {
            StopOrleansClient();
            StopWebHost();
        }

        private static async Task StartOrleansClient(string[] args)
        {
            _log.Information("Starting orleans client");
            
            var attempts = 0;
            while (true)
            {
                try
                {
                    _orleansClient = BuildOrleansClient(args);

                    attempts++;
                    _log.Information($"Trying to connect to the host, attempt #{attempts}");
                    
                    await _orleansClient.Connect();
                    break;
                }
                catch (Exception ex)
                {
                    _log.Warning(ex, $"Failed to connect to the host, after {attempts} attempt(s)");

                    _orleansClient?.Dispose();
                    if (attempts == 5) throw ex;

                    await Task.Delay(TimeSpan.FromSeconds(5));
                }
            }
            
            _log.Information("Orleans client started");
        }

        private static void StopOrleansClient()
        {
            _log.Information("Stopping orleans client");
            _orleansClient.Dispose();
            _log.Warning("Orleans client stopped");
        }
        
        private static async Task StartWebHost(string[] args)
        {
            _log.Information("Starting web api");
            
            _webHost = BuildWebHost(args, ConfigureServices);

            await _webHost.StartAsync();

            var serverAddresses = _webHost.ServerFeatures.Get<IServerAddressesFeature>()?.Addresses;

            if (serverAddresses != null)
            {
                foreach (var address in serverAddresses)
                {
                    _log.Information($"Now listening on: {address}");
                }
            }
            
            _log.Information("Web api started");
        }

        private static void StopWebHost()
        {
            _log.Information("Stopping web api");
            _webHost.Dispose();
            _log.Warning("Web api stopped");
        }

        private static void ConfigureServices(IServiceCollection services)
        {
            services.AddSingleton(_orleansClient);
            services.AddSingleton<IProjectionsClient>(
                new ProjectionsClient(new EventStoreLogger(), new IPEndPoint(IPAddress.Loopback, 2113), TimeSpan.FromSeconds(10)));
        }
    }
}
