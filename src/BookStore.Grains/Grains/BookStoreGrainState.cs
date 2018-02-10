using System;
using BookStore.Grains.Events.BookStore;

namespace BookStore.Grains.Grains
{
    [Serializable]
    public class BookStoreGrainState
    {
        public Guid Id { get; set; }
        
        public string Name { get; set; }
        
        public void Apply(BookStoreInitializedEvent @event)
        {
            Id = @event.Id;
            Name = @event.Name;
        }
    }
}