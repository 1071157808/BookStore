using System;

namespace BookStore.Grains.Events
{
    [Serializable]
    public class Event
    {
        public Event()
        {
            EventId = Guid.NewGuid();
            TimeStamp = DateTimeOffset.UtcNow;
        }
        
        public Guid EventId { get; set; }
        
        public DateTimeOffset TimeStamp { get; set; }
    }
}