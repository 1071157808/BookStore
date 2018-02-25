using System;
using System.Net;
using System.Threading.Tasks;
using EventStore.ClientAPI;
using EventStore.ClientAPI.Projections;
using EventStore.ClientAPI.SystemData;
using Newtonsoft.Json;

namespace BookStore.Client.EventStore
{
    public class ProjectionsClient : IProjectionsClient
    {
        private readonly ProjectionsManager _manager;

        public ProjectionsClient(ILogger logger, EndPoint endPoint, TimeSpan timeout, string schema = "http")
        {
            _manager = new ProjectionsManager(logger, endPoint, timeout, schema);
        }
        
        public async Task<TState> GetStateAsync<TState>(string name, UserCredentials userCredentials = null)
        {
            var state = await _manager.GetStateAsync(name, userCredentials);
            var result = JsonConvert.DeserializeObject<TState>(state);

            return result;
        }

        public async Task<TState> GetPartitionStateAsync<TState>(string name, string partitionId, UserCredentials userCredentials = null)
        {
            var state = await _manager.GetPartitionStateAsync(name, partitionId, userCredentials);
            var result = JsonConvert.DeserializeObject<TState>(state);

            return result;
        }
    }
}