using System.Threading.Tasks;
using BookStore.Contracts.Commands.BookStoreGrain;
using Orleans;

namespace BookStore.Contracts.Grains
{
    public interface IBookStoreGrain : IGrainWithGuidKey
    {
        Task Initialize(InitializeBookStoreCommand cmd);

        Task Update(UpdateBookStoreCommand cmd);
    }
}