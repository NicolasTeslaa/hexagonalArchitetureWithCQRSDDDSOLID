using Domain.Book.Entities;
using Domain.Book.Ports;
using Domain.Guest.Entities;
using Domain.Room.Entities;
using Microsoft.EntityFrameworkCore;

namespace Data.MySql.Repositories;

public class BookRepository : IBookRepository
{
    private readonly HotelDbContext _context;

    public BookRepository(HotelDbContext context) => _context = context;

    public async Task<Booking> CreateBookAsync(Booking book)
    {
        _context.Book.Add(book);

        await _context.SaveChangesAsync();

        return book;
    }

    public async Task<bool> DeleteBookAsync(int id)
    {
        var book = _context.Book.FirstOrDefault(b => b.Id == id);

        if (book is null) return false;

        _context.Book.Remove(book);

        await _context.SaveChangesAsync();

        return true;
    }

    public async Task<Booking?> GetBookByIdAsync(int id)
      => await _context.Book.FindAsync(id);

    public async Task<(Room?, Guest?)> GetRoomAndGuestAsync(int roomId, int guestId)
    {
        var room = await _context.Room.Include(r => r.Bookings)
            .Where(r => r.Id == roomId)
            .FirstOrDefaultAsync();

        var guest = await _context.Guest.FindAsync(guestId);

        var result = (room, guest);

        return result;
    }

    public Task<bool> HasConflictAsync(int id, DateTime start, DateTime end, object value)
    {
        var query = _context.Book.Where(b => b.RoomId == id && b.Start < end && b.End > start);

        if (value is int bookId)
            query = query.Where(b => b.Id != bookId);

        return Task.FromResult(query.Any());
    }

    public async Task<Booking?> UpdateBookAsync(Booking book)
    {
        var existingBook = _context.Book.FirstOrDefault(b => b.Id == book.Id);

        if (existingBook is null) return null;

        existingBook.PlacedAt = book.PlacedAt;
        existingBook.Start = book.Start;
        existingBook.End = book.End;
        existingBook.RoomId = book.RoomId;
        existingBook.GuestId = book.GuestId;
        existingBook.SetStatus(book.CurrentStatus);

        _context.Book.Update(existingBook);

        await _context.SaveChangesAsync();

        return existingBook;
    }
}