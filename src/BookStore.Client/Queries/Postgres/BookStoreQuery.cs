using System;
using System.Threading.Tasks;
using AutoMapper;
using BookStore.Client.Responses.BookStore;
using BookStore.ProjectionBuilder.Postgres.Database;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json.Linq;

namespace BookStore.Client.Queries.Postgres
{
    public class BookStoreQuery : IBookStoreQuery
    {
        private static readonly MapperConfiguration _config = new MapperConfiguration(c => c.AddProfile<BookStoreProfile>());
        private static readonly IMapper _mapper = _config.CreateMapper();
        private readonly string _connectionString;
        
        public BookStoreQuery(IConfiguration config)
        {
            if (config == null) throw new ArgumentNullException(nameof(config));
            
            _connectionString = config.GetConnectionString("bookstore_projection");
        }
        
        public async Task<BookStoreResponse[]> GetBookStores()
        {
            using (var db = new ApplicationDbContext(_connectionString))
            {
                var entities = await db.BookStores.ToArrayAsync();
                var bookStores = _mapper.Map<BookStoreResponse[]>(entities);
                
                return bookStores;
            }
        }

        public async Task<BookStoreResponse> GetBookStore(Guid id)
        {
            using (var db = new ApplicationDbContext(_connectionString))
            {
                var entity = await db.BookStores.FindAsync(id);
                var bookStore = _mapper.Map<BookStoreResponse>(entity);
                
                return bookStore;
            }
        }
        
        public class BookStoreProfile : Profile
        {
            public BookStoreProfile()
            {
                CreateMap<ProjectionBuilder.Postgres.Database.Entities.BookStore, BookStoreResponse>()
                    .ForMember(d => d.Id, o => o.MapFrom(s => s.Id))
                    .ForMember(d => d.Name, o => o.MapFrom(s => s.Name))
                    .ForMember(d => d.Address, o => o.ResolveUsing(e =>
                    {
                        var address = JObject.Parse(e.Address);
                        
                        return new AddressResponse
                        {
                            Country = address.Value<string>("Country"),
                            City = address.Value<string>("City"),
                            Street = address.Value<string>("Street"),
                            Building = address.Value<string>("Building")
                        };
                    }));
            }
        }
    }
}