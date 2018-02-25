namespace BookStore.Contracts.Commands.BookStoreGrain
{
    public class AddressCommandData
    {
        public string Country { get; set; }

        public string City { get; set; }

        public string Street { get; set; }

        public string Building { get; set; }
    }
}