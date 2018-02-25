using System;

namespace BookStore.Contracts.Commands.BookStoreGrain
{
    public class InitializeBookStoreCommand
    {
        public Guid Id { get; set; }
        
        public string Name { get; set; }
        
        public AddressCommandData Address { get; set; }
    }
}