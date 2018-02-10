using System;
using System.Threading.Tasks;
using BookStore.Contracts.Grains;
using Orleans;

namespace BookStore.Grains.Grains
{
    public class PingGrain : Grain, IPingGrain
    {
        public Task<string> Ping()
        {           
            return Task.FromResult($"Pong: {DateTimeOffset.Now:HH:mm:ss.fff}");
        }
    }
}
