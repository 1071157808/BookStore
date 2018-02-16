using System.Threading.Tasks;
using BookStore.Contracts.Commands.BookStoreGrain;
using BookStore.Contracts.Grains;
using BookStore.Grains.Events.BookStore;
using Orleans.Providers;

namespace BookStore.Grains.Grains
{
    [LogConsistencyProvider(ProviderName = "CustomStorage")]
    public class BookStoreGrain : EventStoreGrain<BookStoreGrainState>, IBookStoreGrain
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
    }
}