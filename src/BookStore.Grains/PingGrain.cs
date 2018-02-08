using System;
using System.Threading.Tasks;
using BookStore.Contracts;
using Orleans;

namespace BookStore.Grains
{
    public class PingGrain : Grain, IPingGrain
    {
        public Task<string> Ping()
        {           
            return Task.FromResult($"Pong: {DateTimeOffset.Now.ToString("HH:mm:ss.fff")}");
        }
    }
}
