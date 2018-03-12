namespace BookStore.Grains.Events.BookStore
{
    public class AddressEventData
    {
        public AddressEventData(string country, string city, string street, string building)
        {
            Country = country;
            City = city;
            Street = street;
            Building = building;
        }
        
        public string Country { get; private set; }

        public string City { get; private set; }

        public string Street { get; private set; }

        public string Building { get; private set; }
    }
}