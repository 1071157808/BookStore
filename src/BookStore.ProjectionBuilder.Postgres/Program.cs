using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using BookStore.ProjectionBuilder.Postgres.Database;
using BookStore.ProjectionBuilder.Postgres.EventStore;
using BookStore.ProjectionBuilder.Postgres.Handlers;
using EventStore.ClientAPI;
using EventStore.ClientAPI.SystemData;
using Serilog;
using Serilog.Sinks.SystemConsole.Themes;
using ILogger = Serilog.ILogger;

namespace BookStore.ProjectionBuilder.Postgres
{
    class Program
    {
        private static ILogger _log;
        private static IEventStoreConnection _eventStoreConnection;
        private static EventStoreStreamCatchUpSubscription[] _eventStoreSubscriptions;

        private static readonly Dictionary<string, BaseHandler> _knownStreams = new Dictionary<string, BaseHandler>
        {
            {"$ce-BookStoreGrain", new BookStoreGrainHandler("$ce-BookStoreGrain")}
        };

        private static readonly ManualResetEventSlim _wait = new ManualResetEventSlim(false);

        static async Task Main(string[] args)
        {
            Log.Logger = new LoggerConfiguration()
                .MinimumLevel.Debug()
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
                eventArgs.Cancel = true;
            };

            await Startup(args);

            _wait.Wait();
        }

        private static async Task Startup(string[] args)
        {
            // database
            var streamVersions = await InitializeStremVersions(_knownStreams.Keys.ToArray());

            // event store
            await StartEventStoreConnection(args);
            Subscribe(streamVersions);
        }

        private static void Shutdown()
        {
            Unsubscribe();
            StopEventStoreConnection();
            
            Log.CloseAndFlush();
        }

        // database
        private static async Task<(string StreamName, long Version)[]> InitializeStremVersions(string[] knownStreams)
        {
            _log.Information("Initializing known streams versions in database");

            await StreamVersionsManager.InitializeStreamVersions(knownStreams);
            var streamVersions = await StreamVersionsManager.GetStreamVersions();

            return streamVersions;
        }

        // event store connection
        private static IEventStoreConnection BuildEventStoreConnection(string[] args)
        {
            var settings = ConnectionSettings.Create()
                .SetDefaultUserCredentials(new UserCredentials("admin", "admin"))
                .UseCustomLogger(new EventStoreLogger());

            return EventStoreConnection.Create(settings, new IPEndPoint(IPAddress.Loopback, 1113));
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

        // subsriptions
        private static void Subscribe((string StreamName, long Version)[] streamVersions)
        {
            _eventStoreSubscriptions = new EventStoreStreamCatchUpSubscription[streamVersions.Length];

            for (var i = 0; i < streamVersions.Length; i++)
            {
                var streamName = streamVersions[i].StreamName;
                var streamVersion = streamVersions[i].Version;

                _log.Debug($"Subscribing to stream: {streamName}, version: {streamVersion}");

                _eventStoreSubscriptions[i] = _eventStoreConnection.SubscribeToStreamFrom(
                    streamName,
                    streamVersion,
                    CatchUpSubscriptionSettings.Default,
                    async (subscription, e) => { await _knownStreams[streamName].Handle(e); });
            }
        }

        private static void Unsubscribe()
        {
            foreach (var eventStoreSubscription in _eventStoreSubscriptions)
            {
                eventStoreSubscription.Stop();
            }
        }
    }
}