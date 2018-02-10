using System;

namespace BookStore.Grains.Events
{
    public abstract class Event
    {
        protected Event()
        {
            EventId = Guid.NewGuid();
            TimeStamp = DateTimeOffset.UtcNow;
        }
        
        public Guid EventId { get; private set; }
        
        public DateTimeOffset TimeStamp { get; private set; }
    }
}