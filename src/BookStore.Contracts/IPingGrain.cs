using System.Threading.Tasks;
using Orleans;

namespace BookStore.Contracts
{
    public interface IPingGrain : IGrainWithGuidKey
    {
        Task<string> Ping();
    }
}
