using Application.Book.DTO;
using Application.Book.Port;
using Application.Book.Request;
using Application.Book.Responses;
using Domain.Book.Exceptions;
using Domain.Book.Ports;

namespace Application.Book.Services;

public class BookManager : IBookManager
{
    private readonly IBookRepository _bookRepository;
    public BookManager(IBookRepository bookRepository) => _bookRepository = bookRepository;

    public async Task<BookResponse> CreateBookAsync(BookDTO book)
    {
        try
        {
            var bookEntity = BookDTO.MapToEntity(book);

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

    public async Task<BookResponse> GetById(int id)
    {
        var book = await _bookRepository.GetBookByIdAsync(id);

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
