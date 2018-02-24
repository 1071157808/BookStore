using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BookStore.Grains.Events;
using EventStore.ClientAPI;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Orleans;
using Orleans.EventSourcing;
using Orleans.EventSourcing.CustomStorage;

namespace BookStore.Grains
{
    public class EventStoreGrain<TState> : JournaledGrain<TState, Event>, ICustomStorageInterface<TState, Event>
        where TState : class, new()
    {
        private const string _eventClrTypeKey = "EventClrType";
        private readonly IEventStoreConnection _connection;
        private string _stream;

        protected EventStoreGrain(IEventStoreConnection connection)
        {
            _connection = connection;
        }

        public async Task<KeyValuePair<int, TState>> ReadStateFromStorage()
        {
            var state = new TState();
            StreamEventsSlice currentSlice;
            var nextSliceStart = StreamPosition.Start;

            do
            {
                currentSlice = await _connection.ReadStreamEventsForwardAsync(_stream, nextSliceStart, 10, false);

                foreach (var resolvedEvent in currentSlice.Events)
                {
                    var @event = DeserializeEvent(resolvedEvent.Event);
                    ((dynamic) state).Apply((dynamic) @event);
                }

                nextSliceStart = (int) currentSlice.NextEventNumber;
            } while (!currentSlice.IsEndOfStream);

            var version = currentSlice.Status == SliceReadStatus.Success ? currentSlice.LastEventNumber + 1 : 0;
            return new KeyValuePair<int, TState>((int) version, state);
        }

        public async Task<bool> ApplyUpdatesToStorage(IReadOnlyList<Event> updates, int expectedversion)
        {
            expectedversion = expectedversion - 1;
            var eventData = updates.Select(ToEventData).ToArray();
            var result = await _connection.ConditionalAppendToStreamAsync(_stream, expectedversion, eventData);

            return result.Status != ConditionalWriteStatus.VersionMismatch;
        }

        public override async Task OnActivateAsync()
        {
            _stream = $"{GetType().Name}-{this.GetPrimaryKey()}";
            await base.OnActivateAsync();
        }

        private static EventData ToEventData(Event @event)
        {
            var eventType = @event.GetType();
            var eventHeaders = new Dictionary<string, object> {[_eventClrTypeKey] = eventType.AssemblyQualifiedName};

            var data = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(@event));
            var metadata = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(eventHeaders));

            return new EventData(@event.EventId, eventType.Name, true, data, metadata);
        }

        private static object DeserializeEvent(RecordedEvent evntData)
        {
            var metadata = Encoding.UTF8.GetString(evntData.Metadata);
            var data = Encoding.UTF8.GetString(evntData.Data);

            var eventClrTypeName = JObject.Parse(metadata).Value<string>(_eventClrTypeKey);
            return JsonConvert.DeserializeObject(data, Type.GetType(eventClrTypeName));
        }
    }
}