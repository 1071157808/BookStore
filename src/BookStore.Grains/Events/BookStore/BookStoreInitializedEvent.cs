using System;

namespace BookStore.Grains.Events.BookStore
{
    [Serializable]
    public class BookStoreInitializedEvent : Event
    {
        public BookStoreInitializedEvent(Guid id, string name)
        {
            Id = id;
            Name = name;
        }

        public Guid Id { get; set; }
        
        public string Name { get; set; }
    }
}