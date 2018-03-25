using System;
using System.Threading.Tasks;
using BookStore.Client.Responses.BookStore;

namespace BookStore.Client.Queries
{
    public interface IBookStoreQuery
    {
        Task<BookStoreResponse[]> GetBookStores();

        Task<BookStoreResponse> GetBookStore(Guid id);
    }
}