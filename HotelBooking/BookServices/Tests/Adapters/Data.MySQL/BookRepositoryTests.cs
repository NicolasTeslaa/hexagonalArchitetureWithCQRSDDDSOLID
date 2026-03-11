using System.Reflection;
using Data.MySql;
using Data.MySql.Repositories;
using Domain.Book.Entities;
using Domain.Book.Enums;
using Domain.Guest.Entities;
using Domain.Guest.ValueObjects;
using Domain.Room.Entities;
using Domain.Room.Enums;
using Domain.Room.ValueObjects;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace Data.MySQL;

public class BookRepositoryTests
{
    private HotelDbContext CreateContext()
    {
        var options = new DbContextOptionsBuilder<HotelDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        return new HotelDbContext(options);
    }

    private static Room CreateRoom(
        int id = 0,
        string name = "Quarto Luxo",
        int level = 1,
        bool inMaintenance = false,
        decimal priceValue = 150)
    {
        return new Room
        {
            Id = id,
            Name = name,
            Level = level,
            InMaintenance = inMaintenance,
            Price = new Price
            {
                Value = priceValue,
                Currency = AcceptedCurrencies.Dollars
            },
            Bookings = new List<Booking>()
        };
    }

    private static Guest CreateGuest(
        int id = 0,
        string name = "Tesla Silva",
        string email = "tesla@gmail.com")
    {
        return new Guest
        {
            Id = id,
            Name = name,
            Email = email,
            DocumentId = new PersonId
            {
                IdNumber = "12345678901",
                Document = DocumentType.Passport
            }
        };
    }

    private static Booking CreateBooking(
        int roomId,
        int guestId,
        DateTime? start = null,
        DateTime? end = null,
        DateTime? placedAt = null)
    {
        return new Booking
        {
            RoomId = roomId,
            GuestId = guestId,
            PlacedAt = placedAt ?? DateTime.UtcNow,
            Start = start ?? DateTime.UtcNow.AddHours(1),
            End = end ?? DateTime.UtcNow.AddHours(2)
        };
    }

    private static void SetBookingStatus(Booking booking, Status status)
    {
        var property = typeof(Booking).GetProperty(
            "Status",
            BindingFlags.Instance | BindingFlags.NonPublic);

        property!.SetValue(booking, status);
    }

    [Fact]
    public async Task CreateBookAsync_Should_Add_Booking_And_Return_Entity()
    {
        using var context = CreateContext();

        var room = CreateRoom();
        var guest = CreateGuest();

        context.Room.Add(room);
        context.Guest.Add(guest);
        await context.SaveChangesAsync();

        var repository = new BookRepository(context);

        var booking = CreateBooking(room.Id, guest.Id);
        SetBookingStatus(booking, Status.Created);

        var result = await repository.CreateBookAsync(booking);

        Assert.NotNull(result);
        Assert.True(result.Id > 0);

        var savedBooking = await context.Book.FirstOrDefaultAsync(b => b.Id == result.Id);
        Assert.NotNull(savedBooking);
        Assert.Equal(room.Id, savedBooking!.RoomId);
        Assert.Equal(guest.Id, savedBooking.GuestId);
        Assert.Equal(booking.Start, savedBooking.Start);
        Assert.Equal(booking.End, savedBooking.End);
    }

    [Fact]
    public async Task DeleteBookAsync_Should_Return_True_And_Remove_Booking_When_Found()
    {
        using var context = CreateContext();

        var room = CreateRoom();
        var guest = CreateGuest();

        context.Room.Add(room);
        context.Guest.Add(guest);
        await context.SaveChangesAsync();

        var booking = CreateBooking(room.Id, guest.Id);
        context.Book.Add(booking);
        await context.SaveChangesAsync();

        var repository = new BookRepository(context);

        var result = await repository.DeleteBookAsync(booking.Id);

        Assert.True(result);

        var deletedBooking = await context.Book.FindAsync(booking.Id);
        Assert.Null(deletedBooking);
    }

    [Fact]
    public async Task DeleteBookAsync_Should_Return_False_When_Booking_Not_Found()
    {
        using var context = CreateContext();
        var repository = new BookRepository(context);

        var result = await repository.DeleteBookAsync(999);

        Assert.False(result);
    }

    [Fact]
    public async Task GetBookByIdAsync_Should_Return_Booking_When_Found()
    {
        using var context = CreateContext();

        var room = CreateRoom();
        var guest = CreateGuest();

        context.Room.Add(room);
        context.Guest.Add(guest);
        await context.SaveChangesAsync();

        var booking = CreateBooking(room.Id, guest.Id);
        context.Book.Add(booking);
        await context.SaveChangesAsync();

        var repository = new BookRepository(context);

        var result = await repository.GetBookByIdAsync(booking.Id);

        Assert.NotNull(result);
        Assert.Equal(booking.Id, result!.Id);
        Assert.Equal(room.Id, result.RoomId);
        Assert.Equal(guest.Id, result.GuestId);
    }

    [Fact]
    public async Task GetBookByIdAsync_Should_Return_Null_When_Not_Found()
    {
        using var context = CreateContext();
        var repository = new BookRepository(context);

        var result = await repository.GetBookByIdAsync(999);

        Assert.Null(result);
    }

    [Fact]
    public async Task GetRoomAndGuestAsync_Should_Return_Room_And_Guest_When_Both_Exist()
    {
        using var context = CreateContext();

        var room = CreateRoom(name: "Suite Master");
        var guest = CreateGuest(name: "Nicolas");

        context.Room.Add(room);
        context.Guest.Add(guest);
        await context.SaveChangesAsync();

        var booking1 = CreateBooking(room.Id, guest.Id);
        var booking2 = CreateBooking(room.Id, guest.Id, DateTime.UtcNow.AddDays(1), DateTime.UtcNow.AddDays(2));

        context.Book.AddRange(booking1, booking2);
        await context.SaveChangesAsync();

        var repository = new BookRepository(context);

        var (resultRoom, resultGuest) = await repository.GetRoomAndGuestAsync(room.Id, guest.Id);

        Assert.NotNull(resultRoom);
        Assert.NotNull(resultGuest);
        Assert.Equal(room.Id, resultRoom!.Id);
        Assert.Equal(guest.Id, resultGuest!.Id);
        Assert.NotNull(resultRoom.Bookings);
        Assert.Equal(2, resultRoom.Bookings.Count);
    }

    [Fact]
    public async Task GetRoomAndGuestAsync_Should_Return_Null_Room_When_Room_Does_Not_Exist()
    {
        using var context = CreateContext();

        var guest = CreateGuest();
        context.Guest.Add(guest);
        await context.SaveChangesAsync();

        var repository = new BookRepository(context);

        var (resultRoom, resultGuest) = await repository.GetRoomAndGuestAsync(999, guest.Id);

        Assert.Null(resultRoom);
        Assert.NotNull(resultGuest);
        Assert.Equal(guest.Id, resultGuest!.Id);
    }

    [Fact]
    public async Task GetRoomAndGuestAsync_Should_Return_Null_Guest_When_Guest_Does_Not_Exist()
    {
        using var context = CreateContext();

        var room = CreateRoom();
        context.Room.Add(room);
        await context.SaveChangesAsync();

        var repository = new BookRepository(context);

        var (resultRoom, resultGuest) = await repository.GetRoomAndGuestAsync(room.Id, 999);

        Assert.NotNull(resultRoom);
        Assert.Null(resultGuest);
        Assert.Equal(room.Id, resultRoom!.Id);
    }

    [Fact]
    public async Task HasConflictAsync_Should_Return_True_When_There_Is_Overlapping_Booking()
    {
        using var context = CreateContext();

        var room = CreateRoom();
        var guest = CreateGuest();

        context.Room.Add(room);
        context.Guest.Add(guest);
        await context.SaveChangesAsync();

        var existingBooking = CreateBooking(
            room.Id,
            guest.Id,
            DateTime.UtcNow.AddHours(2),
            DateTime.UtcNow.AddHours(5));

        context.Book.Add(existingBooking);
        await context.SaveChangesAsync();

        var repository = new BookRepository(context);

        var result = await repository.HasConflictAsync(
            room.Id,
            DateTime.UtcNow.AddHours(3),
            DateTime.UtcNow.AddHours(6),
            null!);

        Assert.True(result);
    }

    [Fact]
    public async Task HasConflictAsync_Should_Return_False_When_There_Is_No_Overlap()
    {
        using var context = CreateContext();

        var room = CreateRoom();
        var guest = CreateGuest();

        context.Room.Add(room);
        context.Guest.Add(guest);
        await context.SaveChangesAsync();

        var existingBooking = CreateBooking(
            room.Id,
            guest.Id,
            DateTime.UtcNow.AddHours(2),
            DateTime.UtcNow.AddHours(4));

        context.Book.Add(existingBooking);
        await context.SaveChangesAsync();

        var repository = new BookRepository(context);

        var result = await repository.HasConflictAsync(
            room.Id,
            DateTime.UtcNow.AddHours(5),
            DateTime.UtcNow.AddHours(7),
            null!);

        Assert.False(result);
    }

    [Fact]
    public async Task HasConflictAsync_Should_Return_False_When_Overlap_Is_From_Same_Booking_Being_Updated()
    {
        using var context = CreateContext();

        var room = CreateRoom();
        var guest = CreateGuest();

        context.Room.Add(room);
        context.Guest.Add(guest);
        await context.SaveChangesAsync();

        var existingBooking = CreateBooking(
            room.Id,
            guest.Id,
            DateTime.UtcNow.AddHours(2),
            DateTime.UtcNow.AddHours(5));

        context.Book.Add(existingBooking);
        await context.SaveChangesAsync();

        var repository = new BookRepository(context);

        var result = await repository.HasConflictAsync(
            room.Id,
            DateTime.UtcNow.AddHours(3),
            DateTime.UtcNow.AddHours(4),
            existingBooking.Id);

        Assert.False(result);
    }

    [Fact]
    public async Task HasConflictAsync_Should_Return_False_When_Booking_Is_From_Another_Room()
    {
        using var context = CreateContext();

        var room1 = CreateRoom(name: "Room 1");
        var room2 = CreateRoom(name: "Room 2");
        var guest = CreateGuest();

        context.Room.AddRange(room1, room2);
        context.Guest.Add(guest);
        await context.SaveChangesAsync();

        var existingBooking = CreateBooking(
            room1.Id,
            guest.Id,
            DateTime.UtcNow.AddHours(2),
            DateTime.UtcNow.AddHours(5));

        context.Book.Add(existingBooking);
        await context.SaveChangesAsync();

        var repository = new BookRepository(context);

        var result = await repository.HasConflictAsync(
            room2.Id,
            DateTime.UtcNow.AddHours(3),
            DateTime.UtcNow.AddHours(4),
            null!);

        Assert.False(result);
    }

    [Fact]
    public async Task UpdateBookAsync_Should_Return_Updated_Booking_When_Found()
    {
        using var context = CreateContext();

        var room1 = CreateRoom(name: "Room 1");
        var room2 = CreateRoom(name: "Room 2");
        var guest1 = CreateGuest(name: "Guest 1", email: "guest1@gmail.com");
        var guest2 = CreateGuest(name: "Guest 2", email: "guest2@gmail.com");

        context.Room.AddRange(room1, room2);
        context.Guest.AddRange(guest1, guest2);
        await context.SaveChangesAsync();

        var originalBooking = CreateBooking(
            room1.Id,
            guest1.Id,
            DateTime.UtcNow.AddHours(1),
            DateTime.UtcNow.AddHours(2),
            DateTime.UtcNow.AddMinutes(-30));

        context.Book.Add(originalBooking);
        await context.SaveChangesAsync();

        var repository = new BookRepository(context);

        var updatedBooking = new Booking
        {
            Id = originalBooking.Id,
            PlacedAt = DateTime.UtcNow,
            Start = DateTime.UtcNow.AddHours(5),
            End = DateTime.UtcNow.AddHours(8),
            RoomId = room2.Id,
            GuestId = guest2.Id
        };

        var result = await repository.UpdateBookAsync(updatedBooking);

        Assert.NotNull(result);
        Assert.Equal(originalBooking.Id, result!.Id);
        Assert.Equal(updatedBooking.PlacedAt, result.PlacedAt);
        Assert.Equal(updatedBooking.Start, result.Start);
        Assert.Equal(updatedBooking.End, result.End);
        Assert.Equal(updatedBooking.RoomId, result.RoomId);
        Assert.Equal(updatedBooking.GuestId, result.GuestId);

        var bookingInDb = await context.Book.FindAsync(originalBooking.Id);
        Assert.NotNull(bookingInDb);
        Assert.Equal(updatedBooking.PlacedAt, bookingInDb!.PlacedAt);
        Assert.Equal(updatedBooking.Start, bookingInDb.Start);
        Assert.Equal(updatedBooking.End, bookingInDb.End);
        Assert.Equal(updatedBooking.RoomId, bookingInDb.RoomId);
        Assert.Equal(updatedBooking.GuestId, bookingInDb.GuestId);
    }

    [Fact]
    public async Task UpdateBookAsync_Should_Return_Null_When_Booking_Not_Found()
    {
        using var context = CreateContext();
        var repository = new BookRepository(context);

        var booking = new Booking
        {
            Id = 999,
            PlacedAt = DateTime.UtcNow,
            Start = DateTime.UtcNow.AddHours(1),
            End = DateTime.UtcNow.AddHours(2),
            RoomId = 1,
            GuestId = 1
        };

        var result = await repository.UpdateBookAsync(booking);

        Assert.Null(result);
    }
}