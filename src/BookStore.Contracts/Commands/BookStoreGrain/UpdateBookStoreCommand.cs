using System;

namespace BookStore.Contracts.Commands.BookStoreGrain
{
    public class UpdateBookStoreCommand
    {
        public UpdateBookStoreCommand(Guid id, string name)
        {
            Id = id;
            Name = name;
        }
        
        public Guid Id { get; private set; }

        public string Name { get; private set; }
    }
}