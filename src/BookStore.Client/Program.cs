using System;
using System.Threading.Tasks;
using BookStore.Contracts;
using BookStore.Contracts.Grains;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Hosting.Server.Features;
using Microsoft.Extensions.DependencyInjection;
using Orleans;
using Orleans.Hosting;
using Orleans.Runtime.Configuration;
using Serilog;
using Serilog.Events;
using Serilog.Sinks.SystemConsole.Themes;
using static Orleans.Runtime.Configuration.ClientConfiguration;

namespace BookStore.Client
{
    public class Program
    {
        private static ILogger _log;
        private static IWebHost _webHost;
        private static IClusterClient _orleansClient;
        private static readonly TaskCompletionSource<bool> _wait = new TaskCompletionSource<bool>();

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
                _wait.SetResult(true);
                eventArgs.Cancel = true;
            };

            await Startup(args);

            await _wait.Task;
        }

        private static IClusterClient BuildOrleansClient(string[] args)
        {
            var config = new ClientConfiguration
            {
                ClusterId = "orleans-docker",

                // membership
                AdoInvariant = "Npgsql",
                GatewayProvider = GatewayProviderType.SqlServer,
                DataConnectionString = "Server=localhost;Port=5432;Database=bookstore_membership;User ID=postgres;Pooling=false;",
            };

            return new ClientBuilder()
                .ConfigureApplicationParts(parts => parts.AddApplicationPart(typeof(IPingGrain).Assembly).WithReferences())
                .UseConfiguration(config)
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
            _log.Information("Starting orleans client");
            await StartOrleansClient(args);
            _log.Information("Orleans client started");

            _log.Information("Starting web api");
            await StartWebHost(args);
            _log.Information("Web api started");
        }

        private static void Shutdown()
        {
            _log.Information("Stopping web api");
            _webHost.Dispose();
            _log.Warning("Web api stopped");

            _log.Information("Stopping orleans client");
            _orleansClient.Dispose();
            _log.Warning("Orleans client stopped");
        }

        private static async Task StartOrleansClient(string[] args)
        {
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
        }

        private static async Task StartWebHost(string[] args)
        {
            _webHost = BuildWebHost(args, s => s.Add(new ServiceDescriptor(typeof(IClusterClient), _orleansClient)));

            await _webHost.StartAsync();

            var serverAddresses = _webHost.ServerFeatures.Get<IServerAddressesFeature>()?.Addresses;

            if (serverAddresses != null)
            {
                foreach (var address in serverAddresses)
                {
                    _log.Information($"Now listening on: {address}");
                }
            }
        }
    }
}
