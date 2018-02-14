using System;

namespace BookStore.Grains.Events.BookStore
{
    [Serializable]
    public class BookStoreUpdatedEvent : Event
    {
        public BookStoreUpdatedEvent(string name)
        {
            Name = name;
        }

        public string Name { get; set; }
    }
}