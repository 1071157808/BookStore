using System;
using System.Text;
using System.Threading.Tasks;
using BookStore.Events;
using EventStore.ClientAPI;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Serilog;
using ILogger = Serilog.ILogger;

namespace BookStore.ProjectionBuilder.Postgres.Handlers
{
    internal abstract class BaseHandler
    {
        private const string _eventClrTypeKey = "EventClrType";

        private readonly ILogger _log;
        
        protected BaseHandler()
        {
            _log = Log.ForContext<BaseHandler>();
        }
        
        public async Task Handle(ResolvedEvent resolvedEvent)
        {
            var streamVersion = resolvedEvent.OriginalEventNumber;
            var @event = DeserializeEvent(resolvedEvent.Event);

            try
            {
                _log.Information($"Handle event {@event.GetType().Name}:{@event.EventId}. Stream version: {streamVersion}");
                
                await HandleInternal(@event, streamVersion);
                
                _log.Information($"Event {@event.GetType().Name}:{@event.EventId} handled");
            }
            catch (Exception ex)
            {
                _log.Error(ex, $"Event {@event.GetType().Name}:{@event.EventId} handling error");
            }
        }

        protected abstract Task HandleInternal(object @event, long streamVersion);

        private static Event DeserializeEvent(RecordedEvent evntData)
        {
            var metadata = Encoding.UTF8.GetString(evntData.Metadata);
            var data = Encoding.UTF8.GetString(evntData.Data);
            
            var eventClrTypeName = JObject.Parse(metadata).Value<string>(_eventClrTypeKey);
            return (Event) JsonConvert.DeserializeObject(data, Type.GetType(eventClrTypeName));
        }
    }
}