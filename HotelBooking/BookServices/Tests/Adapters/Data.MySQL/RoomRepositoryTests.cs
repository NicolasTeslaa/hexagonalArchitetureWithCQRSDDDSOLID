using Data.MySql;
using Data.MySql.Repositories;
using Domain.Book.Entities;
using Domain.Room.Entities;
using Domain.Room.Enums;
using Domain.Room.ValueObjects;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace Data.MySQL;

public class RoomRepositoryTests
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

    private static Booking CreateBooking(
        int roomId,
        int guestId = 1,
        DateTime? start = null,
        DateTime? end = null)
    {
        return new Booking
        {
            RoomId = roomId,
            GuestId = guestId,
            PlacedAt = DateTime.UtcNow,
            Start = start ?? DateTime.UtcNow.AddHours(1),
            End = end ?? DateTime.UtcNow.AddHours(2)
        };
    }

    [Fact]
    public async Task AddRoomAsync_Should_Add_Room_And_Return_Id()
    {
        using var context = CreateContext();
        var repository = new RoomRepository(context);
        var room = CreateRoom();

        var result = await repository.AddRoomAsync(room);

        Assert.True(result > 0);

        var savedRoom = await context.Room.FirstOrDefaultAsync(r => r.Id == result);
        Assert.NotNull(savedRoom);
        Assert.Equal("Quarto Luxo", savedRoom!.Name);
        Assert.Equal(1, savedRoom.Level);
        Assert.False(savedRoom.InMaintenance);
        Assert.Equal(150, savedRoom.Price.Value);
        Assert.Equal(AcceptedCurrencies.Dollars, savedRoom.Price.Currency);
    }

    [Fact]
    public async Task GetRoomByIdAsync_Should_Return_Room_When_Found()
    {
        using var context = CreateContext();
        var room = CreateRoom(name: "Quarto Master");
        context.Room.Add(room);
        await context.SaveChangesAsync();

        var repository = new RoomRepository(context);

        var result = await repository.GetRoomByIdAsync(room.Id);

        Assert.NotNull(result);
        Assert.Equal(room.Id, result!.Id);
        Assert.Equal("Quarto Master", result.Name);
    }

    [Fact]
    public async Task GetRoomByIdAsync_Should_Return_Null_When_Not_Found()
    {
        using var context = CreateContext();
        var repository = new RoomRepository(context);

        var result = await repository.GetRoomByIdAsync(999);

        Assert.Null(result);
    }

    [Fact]
    public async Task GetRoomByIdAsync_Should_Load_Bookings_When_They_Exist()
    {
        using var context = CreateContext();

        var room = CreateRoom(name: "Quarto com reservas");
        context.Room.Add(room);
        await context.SaveChangesAsync();

        var booking1 = CreateBooking(room.Id, guestId: 1);
        var booking2 = CreateBooking(room.Id, guestId: 2, start: DateTime.UtcNow.AddDays(1), end: DateTime.UtcNow.AddDays(2));

        context.Set<Booking>().AddRange(booking1, booking2);
        await context.SaveChangesAsync();

        var repository = new RoomRepository(context);

        var result = await repository.GetRoomByIdAsync(room.Id);

        Assert.NotNull(result);
        Assert.NotNull(result!.Bookings);
        Assert.Equal(2, result.Bookings.Count);
    }

    [Fact]
    public async Task GetAllRoomsAsync_Should_Return_All_Rooms()
    {
        using var context = CreateContext();
        context.Room.AddRange(
            CreateRoom(name: "Room 1"),
            CreateRoom(name: "Room 2"),
            CreateRoom(name: "Room 3")
        );
        await context.SaveChangesAsync();

        var repository = new RoomRepository(context);

        var result = await repository.GetAllRoomsAsync();

        Assert.NotNull(result);
        Assert.Equal(3, result.Count());
    }

    [Fact]
    public async Task GetAllRoomsAsync_Should_Load_Bookings()
    {
        using var context = CreateContext();

        var room1 = CreateRoom(name: "Room 1");
        var room2 = CreateRoom(name: "Room 2");

        context.Room.AddRange(room1, room2);
        await context.SaveChangesAsync();

        context.Set<Booking>().AddRange(
            CreateBooking(room1.Id, guestId: 1),
            CreateBooking(room1.Id, guestId: 2),
            CreateBooking(room2.Id, guestId: 3)
        );
        await context.SaveChangesAsync();

        var repository = new RoomRepository(context);

        var result = (await repository.GetAllRoomsAsync()).ToList();

        Assert.Equal(2, result.Count);
        Assert.All(result, room => Assert.NotNull(room.Bookings));

        var firstRoom = result.First(r => r.Id == room1.Id);
        var secondRoom = result.First(r => r.Id == room2.Id);

        Assert.Equal(2, firstRoom.Bookings.Count);
        Assert.Single(secondRoom.Bookings);
    }

    [Fact]
    public async Task UpdateRoomAsync_Should_Return_True_And_Update_Room_When_Found()
    {
        using var context = CreateContext();
        var originalRoom = CreateRoom(name: "Quarto Simples", level: 1, inMaintenance: false, priceValue: 100);
        context.Room.Add(originalRoom);
        await context.SaveChangesAsync();

        var repository = new RoomRepository(context);

        var updatedRoom = new Room
        {
            Id = originalRoom.Id,
            Name = "Quarto Premium",
            Level = 3,
            InMaintenance = true,
            Price = new Price
            {
                Value = 300,
                Currency = AcceptedCurrencies.Bitcoin
            }
        };

        var result = await repository.UpdateRoomAsync(updatedRoom);

        Assert.True(result);

        var roomInDb = await context.Room.FindAsync(originalRoom.Id);
        Assert.NotNull(roomInDb);
        Assert.Equal("Quarto Premium", roomInDb!.Name);
        Assert.Equal(3, roomInDb.Level);
        Assert.True(roomInDb.InMaintenance);
        Assert.Equal(300, roomInDb.Price.Value);
        Assert.Equal(AcceptedCurrencies.Bitcoin, roomInDb.Price.Currency);
    }

    [Fact]
    public async Task UpdateRoomAsync_Should_Return_False_When_Room_Not_Found()
    {
        using var context = CreateContext();
        var repository = new RoomRepository(context);

        var room = new Room
        {
            Id = 999,
            Name = "Inexistente",
            Level = 2,
            InMaintenance = false,
            Price = new Price
            {
                Value = 200,
                Currency = AcceptedCurrencies.Real
            }
        };

        var result = await repository.UpdateRoomAsync(room);

        Assert.False(result);
    }

    [Fact]
    public async Task DeleteRoomAsync_Should_Return_True_And_Remove_Room_When_Found()
    {
        using var context = CreateContext();
        var room = CreateRoom(name: "Quarto para excluir");
        context.Room.Add(room);
        await context.SaveChangesAsync();

        var repository = new RoomRepository(context);

        var result = await repository.DeleteRoomAsync(room.Id);

        Assert.True(result);

        var deletedRoom = await context.Room.FindAsync(room.Id);
        Assert.Null(deletedRoom);
    }

    [Fact]
    public async Task DeleteRoomAsync_Should_Return_False_When_Room_Not_Found()
    {
        using var context = CreateContext();
        var repository = new RoomRepository(context);

        var result = await repository.DeleteRoomAsync(999);

        Assert.False(result);
    }
}