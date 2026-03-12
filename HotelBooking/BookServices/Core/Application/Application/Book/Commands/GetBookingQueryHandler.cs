using Application.Book.DTO;
using Application.Book.Responses;
using Domain.Book.Ports;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Book.Commands
{
    public class GetBookingQueryHandler : IRequestHandler<GetBookingQuery, BookResponse>
    {
        private readonly IBookRepository _bookRepository;
        public GetBookingQueryHandler(IBookRepository bookRepository)
        => _bookRepository = bookRepository;

        public async Task<BookResponse> Handle(GetBookingQuery request, CancellationToken cancellationToken)
        {
            var book = await _bookRepository.GetBookByIdAsync(request.Id);

            if (book is not null)
            {
                return new BookResponse
                {
                    Success = true,
                    Data = BookDTO.MapFromEntity(book)
                };
            }

            return new BookResponse
            {
                Success = false,
                ErrorCode = ErrorCodes.NOT_FOUND,
                Message = "Book not found."
            };
        }
    }
}
