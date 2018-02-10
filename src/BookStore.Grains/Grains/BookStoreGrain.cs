using System.Threading.Tasks;
using BookStore.Contracts.Commands.BookStoreGrain;
using BookStore.Contracts.Grains;
using BookStore.Grains.Events;
using BookStore.Grains.Events.BookStore;
using Orleans;
using Orleans.EventSourcing;
using Orleans.Providers;

namespace BookStore.Grains.Grains
{
    [StorageProvider(ProviderName = "AdoNetStorage")]
    [LogConsistencyProvider(ProviderName = "LogStorage")]
    public class BookStoreGrain : JournaledGrain<BookStoreGrainState, Event>, IBookStoreGrain
    {
        public async Task Initialize(InitializeBookStoreCommand cmd)
        {
            var @event = new BookStoreInitializedEvent(cmd.Id, cmd.Name);
            
            RaiseEvent(@event);
            await ConfirmEvents();
        }
    }
}