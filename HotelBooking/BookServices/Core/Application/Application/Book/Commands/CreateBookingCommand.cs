using Application.Book.DTO;
using Application.Book.Responses;
using MediatR;

namespace Application.Book.Commands;

public class CreateBookingCommand : IRequest<BookResponse>
{
   public BookDTO BookingDTO { get; set; }
}
