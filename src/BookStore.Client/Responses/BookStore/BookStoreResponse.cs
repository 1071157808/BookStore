using System;

namespace BookStore.Client.Responses.BookStore
{
    public class BookStoreResponse
    {
        public Guid Id { get; set; }
        
        public string Name { get; set; }

        public AddressResponse Address { get; set; }
    }
}