using Application.Book.DTO;
using Application.Book.Request;
using Application.Book.Responses;

namespace Application.Book.Port;

public interface IBookManager
{
    Task<BookResponse> CreateBookAsync (BookDTO book);
    Task<BookResponse> GetById(int id);
}
