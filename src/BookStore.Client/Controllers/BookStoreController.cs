using System;
using System.Threading.Tasks;
using BookStore.Client.Queries;
using BookStore.Client.Requests.BookStore;
using BookStore.Client.Responses.BookStore;
using BookStore.Contracts.Commands.BookStoreGrain;
using BookStore.Contracts.Grains;
using Microsoft.AspNetCore.Mvc;
using Orleans;

namespace BookStore.Client.Controllers
{
    [Route("api/[controller]")]
    public class BookStoreController : Controller
    {
        private readonly IClusterClient _client;
        private readonly IBookStoreQuery _query;

        public BookStoreController(IClusterClient client, IBookStoreQuery query)
        {
            _client = client ?? throw new ArgumentNullException(nameof(client));
            _query = query ?? throw new ArgumentNullException(nameof(query));
        }
        
        /// <summary>
        ///      Get book stores data.
        /// </summary>
        [HttpGet]
        public async Task<BookStoreResponse[]> Get()
        {
            var bookStores = await _query.GetBookStores();
            
            return bookStores;
        }

        /// <summary>
        ///      Get book store data.
        /// </summary>
        [HttpGet("{id:guid}")]
        public async Task<BookStoreResponse> Get(Guid id)
        {
            var bookStore = await _query.GetBookStore(id);
            
            return bookStore;
        }
        
        /// <summary>
        ///     Initialize new book store.
        /// </summary>
        [HttpPost]
        public async Task<Guid> Post([FromBody] InitializeBookStoreRequest request)
        {
            var bookStoreId = Guid.NewGuid();
            var cmd = new InitializeBookStoreCommand
            {
                Id = bookStoreId,
                Name = request.Name,
                Address = new AddressCommandData
                {
                    Country = request.Address.Country,
                    City = request.Address.City,
                    Street = request.Address.Street,
                    Building = request.Address.Building
                }
            };
            var bookStore = _client.GetGrain<IBookStoreGrain>(cmd.Id);
            await bookStore.Initialize(cmd);

            return bookStoreId;
        }

        /// <summary>
        ///     Update book store.
        /// </summary>
        [HttpPut("{id:guid}")]
        public async Task Put([FromRoute] Guid id, [FromBody] UpdateBookStoreRequest request)
        {
            var cmd = new UpdateBookStoreCommand
            {
                Id = id,
                Name = request.Name,
                Address = new AddressCommandData
                {
                    Country = request.Address.Country,
                    City = request.Address.City,
                    Street = request.Address.Street,
                    Building = request.Address.Building
                }
            };
            var bookStore = _client.GetGrain<IBookStoreGrain>(cmd.Id);
            await bookStore.Update(cmd);
        }
    }
}