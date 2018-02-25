using System.Threading.Tasks;
using BookStore.Contracts.Commands.BookStoreGrain;
using BookStore.Contracts.Grains;
using BookStore.Grains.Events.BookStore;
using EventStore.ClientAPI;
using Orleans.Providers;

namespace BookStore.Grains.Grains
{
    [LogConsistencyProvider(ProviderName = "CustomStorage")]
    public class BookStoreGrain : EventStoreGrain<BookStoreGrainState>, IBookStoreGrain
    {
        public BookStoreGrain(IEventStoreConnection connection) : base(connection)
        {
        }
        
        public async Task Initialize(InitializeBookStoreCommand cmd)
        {
            var @event = new BookStoreInitializedEvent(
                cmd.Id,
                cmd.Name,
                new AddressEventData(cmd.Address.Country, cmd.Address.City, cmd.Address.Street, cmd.Address.Building));

            RaiseEvent(@event);
            await ConfirmEvents();
        }

        public async Task Update(UpdateBookStoreCommand cmd)
        {
            var @event = new BookStoreUpdatedEvent(
                cmd.Name,
                new AddressEventData(cmd.Address.Country, cmd.Address.City, cmd.Address.Street, cmd.Address.Building));

            RaiseEvent(@event);
            await ConfirmEvents();
        }
    }
}