using Domain.Book.Entities;

namespace Domain.Book.Ports;

public interface IBookRepository
{
    Task<Booking> CreateBookAsync(Booking book);
    Task<Booking?> UpdateBookAsync(Booking book);
    Task<bool> DeleteBookAsync(int id);
    Task<Booking?> GetBookByIdAsync(int id);
    Task<bool> HasConflictAsync(int id, DateTime start, DateTime end, object value);

    Task<(Domain.Room.Entities.Room?, Domain.Guest.Entities.Guest?)> GetRoomAndGuestAsync(int roomId, int guestId);
}
