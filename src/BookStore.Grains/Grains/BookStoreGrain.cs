using System;
using System.Threading.Tasks;
using BookStore.Contracts.Commands.BookStoreGrain;
using BookStore.Contracts.Grains;
using BookStore.Grains.Events.BookStore;
using Newtonsoft.Json.Linq;
using Orleans.EventSourcing;
using Orleans.Providers;

namespace BookStore.Grains.Grains
{
    [StorageProvider(ProviderName = "AdoNetStorage")]
    [LogConsistencyProvider(ProviderName = "LogStorage")]
    public class BookStoreGrain : JournaledGrain<BookStoreGrainState>, IBookStoreGrain
    {
        public async Task Initialize(InitializeBookStoreCommand cmd)
        {
            var @event = new BookStoreInitializedEvent(cmd.Id, cmd.Name);

            RaiseEvent(@event);
            await ConfirmEvents();
        }

        public async Task Update(UpdateBookStoreCommand cmd)
        {
            var @event = new BookStoreUpdatedEvent(cmd.Name);

            RaiseEvent(@event);
            await ConfirmEvents();
        }

        protected override void TransitionState(BookStoreGrainState state, object @event)
        {
            dynamic dynamicState = state;
            dynamic dynamicEvent = @event is JObject jobject
                ? jobject.ToObject(Type.GetType(jobject.Value<string>("$type")))
                : @event;

            dynamicState.Apply(dynamicEvent);
        }
    }
}