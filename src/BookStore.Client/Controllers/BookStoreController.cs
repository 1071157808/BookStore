using System;
using System.Threading.Tasks;
using BookStore.Client.EventStore;
using BookStore.Client.Requests;
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
        private readonly IProjectionsClient _projectionsClient;

        public BookStoreController(IClusterClient client, IProjectionsClient projectionsClient)
        {
            _client = client ?? throw new ArgumentNullException(nameof(client));
            _projectionsClient = projectionsClient ?? throw new ArgumentNullException(nameof(projectionsClient));
        }

        /// <summary>
        ///      Get book store data.
        /// </summary>
        [HttpGet("{id:guid}")]
        public async Task<BookStoreResponse> Get(Guid id)
        {
            var partition = $"BookStoreGrain-{id}";
            var state = await _projectionsClient.GetPartitionStateAsync<BookStoreResponse>("BookStore", partition);
            
            return state;
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