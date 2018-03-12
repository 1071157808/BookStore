using System;

namespace BookStore.Events.V1.BookStore
{
    [Serializable]
    public class BookStoreUpdatedEvent : Event
    {
        public BookStoreUpdatedEvent(string name, AddressEventData address)
        {
            Name = name;
            Address = address;
        }

        public string Name { get; private set; }
        
        public AddressEventData Address { get; private set; }
    }
}