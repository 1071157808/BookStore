using System.Threading.Tasks;
using EventStore.ClientAPI.SystemData;

namespace BookStore.Client.EventStore
{
    public interface IProjectionsClient
    {
        Task<TState> GetStateAsync<TState>(string name, UserCredentials userCredentials = null);

        Task<TState> GetPartitionStateAsync<TState>(string name, string partitionId, UserCredentials userCredentials = null);
    }
}