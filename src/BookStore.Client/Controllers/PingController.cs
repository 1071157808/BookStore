using System;
using System.Threading.Tasks;
using BookStore.Contracts;
using BookStore.Contracts.Grains;
using Microsoft.AspNetCore.Mvc;
using Orleans;

namespace BookStore.Client.Controllers
{
    [Route("api/[controller]")]
    public class PingController : Controller
    {
        private readonly IClusterClient _orleansClient;

        public PingController(IClusterClient orleansClient)
        {
            _orleansClient = orleansClient;
        }

        [HttpGet]
        public async Task<string> Get()
        {
            var grain = _orleansClient.GetGrain<IPingGrain>(Guid.NewGuid());
            var result = await grain.Ping();

            return result;
        }
    }
}
