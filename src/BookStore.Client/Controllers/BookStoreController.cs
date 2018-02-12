using System;
using System.Threading.Tasks;
using BookStore.Client.Requests;
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

        public BookStoreController(IClusterClient client)
        {
            _client = client ?? throw new ArgumentNullException(nameof(client));
        }

        /// <summary>
        ///     Initialize new book store.
        /// </summary>
        [HttpPost]
        public async Task<Guid> Post([FromBody] InitializeBookStoreRequest request)
        {
            var bookStoreId = Guid.NewGuid();
            var cmd = new InitializeBookStoreCommand(bookStoreId, request.Name);
            var bookStore = _client.GetGrain<IBookStoreGrain>(cmd.Id);
            await bookStore.Initialize(cmd);

            return bookStoreId;
        }
    }
}