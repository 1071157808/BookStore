using System;

namespace BookStore.Contracts.Commands.BookStoreGrain
{
    public class InitializeBookStoreCommand
    {
        public InitializeBookStoreCommand(Guid id, string name)
        {
            Id = id;
            Name = name;
        }

        public Guid Id { get; }
        
        public string Name { get; }
    }
}