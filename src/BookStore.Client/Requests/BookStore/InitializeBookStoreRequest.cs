namespace BookStore.Client.Requests.BookStore
{
    public class InitializeBookStoreRequest
    {
        public string Name { get; set; }
        
        public AddressRequest Address { get; set; }
    }
}