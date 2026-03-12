using Application.Book.DTO;
using Application.Book.Responses;
using Domain.Book.Exceptions;
using Domain.Book.Ports;
using MediatR;

namespace Application.Book.Commands;

public class CreateBookingHandler : IRequestHandler<CreateBookingCommand, BookResponse>
{
    private readonly IBookRepository _bookRepository;

    public CreateBookingHandler(IBookRepository bookRepository)
    => _bookRepository = bookRepository;

    public async Task<BookResponse> Handle(CreateBookingCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var bookEntity = BookDTO.MapToEntity(request.BookingDTO);

            await bookEntity.Save(_bookRepository);

            return new BookResponse
            {
                Success = true,
                Data = BookDTO.MapFromEntity(bookEntity)
            };
        }
        catch (RoomRequiredException)
        {
            return new BookResponse
            {
                Success = false,
                ErrorCode = ErrorCodes.ROOM_REQUIRED,
                Message = "Room is required."
            };
        }
        catch (GuestRequiredException)
        {
            return new BookResponse
            {
                Success = false,
                ErrorCode = ErrorCodes.GUEST_REQUIRED,
                Message = "Guest is required."
            };
        }
        catch (InvalidBookingDateRangeException)
        {
            return new BookResponse
            {
                Success = false,
                ErrorCode = ErrorCodes.INVALID_BOOKING_DATE_RANGE,
                Message = "Start date must be earlier than end date."
            };
        }
        catch (BookingInPastException)
        {
            return new BookResponse
            {
                Success = false,
                ErrorCode = ErrorCodes.BOOKING_IN_PAST,
                Message = "Booking cannot be created in the past."
            };
        }
        catch (RoomUnavailableException)
        {
            return new BookResponse
            {
                Success = false,
                ErrorCode = ErrorCodes.ROOM_UNAVAILABLE,
                Message = "Room is unavailable."
            };
        }
        catch (BookingConflictException)
        {
            return new BookResponse
            {
                Success = false,
                ErrorCode = ErrorCodes.BOOKING_CONFLICT,
                Message = "There is already a booking conflict for this room and period."
            };
        }
        catch (Exception)
        {
            return new BookResponse
            {
                Success = false,
                ErrorCode = ErrorCodes.UNEXPECTED_ERROR,
                Message = "An unexpected error occurred."
            };
        }
    }
}
