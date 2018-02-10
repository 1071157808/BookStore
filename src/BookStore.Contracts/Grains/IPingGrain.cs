using System.Threading.Tasks;
using Orleans;

namespace BookStore.Contracts.Grains
{
    public interface IPingGrain : IGrainWithGuidKey
    {
        Task<string> Ping();
    }
}
