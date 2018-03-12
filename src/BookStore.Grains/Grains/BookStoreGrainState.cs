using System;
using BookStore.Events.V1.BookStore;

namespace BookStore.Grains.Grains
{
    public class BookStoreGrainState
    {
        public Guid Id { get; set; }
        
        public string Name { get; set; }
        
        public AddressData Address { get; set; }

        public void Apply(BookStoreInitializedEvent @event)
        {
            Id = @event.Id;
            Name = @event.Name;
            Address = new AddressData
            {
                Country = @event.Address.Country,
                City = @event.Address.City,
                Street = @event.Address.Street,
                Building = @event.Address.Building
            };
        }
        
        public void Apply(BookStoreUpdatedEvent @event)
        {
            Name = @event.Name;
            Address.Country = @event.Address.Country;
            Address.City = @event.Address.City;
            Address.Street = @event.Address.Street;
            Address.Building = @event.Address.Building;
        }
        
        public class AddressData
        {
            public string Country { get; set; }

            public string City { get; set; }

            public string Street { get; set; }

            public string Building { get; set; }
        }
    }
}