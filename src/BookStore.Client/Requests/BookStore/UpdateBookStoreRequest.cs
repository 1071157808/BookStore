namespace BookStore.Client.Requests.BookStore
{
    public class UpdateBookStoreRequest
    {
        public string Name { get; set; }

        public AddressRequest Address { get; set; }
    }
}