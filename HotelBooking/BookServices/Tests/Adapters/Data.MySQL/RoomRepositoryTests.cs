using Data.MySql;
using Data.MySql.Repositories;
using Domain.Room.Entities;
using Domain.Room.Enums;
using Microsoft.EntityFrameworkCore;

namespace Data.MySQL;

public class RoomRepositoryTests
{
    private HotelDbContext CreateContext()
    {
        var options = new DbContextOptionsBuilder<HotelDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
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
            Price = new Domain.Room.ValueObjects.Price()
            {
                Value = priceValue,
                Currency = AcceptedCurrencies.Dollars
            }
        };
    }

    [Fact]
    public async Task AddRoomAsync_Should_Add_Room_And_Return_Id()
    {
        // Arrange
        using var context = CreateContext();
        var repository = new RoomRepository(context);
        var room = CreateRoom();

        // Act
        var result = await repository.AddRoomAsync(room);

        // Assert
        Assert.True(result > 0);

        var savedRoom = await context.Room.FirstOrDefaultAsync(r => r.Id == result);
        Assert.NotNull(savedRoom);
        Assert.Equal("Quarto Luxo", savedRoom.Name);
        Assert.Equal(1, savedRoom.Level);
    }

    [Fact]
    public async Task GetRoomByIdAsync_Should_Return_Room_When_Found()
    {
        // Arrange
        using var context = CreateContext();
        var room = CreateRoom(name: "Quarto Master");
        context.Room.Add(room);
        await context.SaveChangesAsync();

        var repository = new RoomRepository(context);

        // Act
        var result = await repository.GetRoomByIdAsync(room.Id);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(room.Id, result.Id);
        Assert.Equal("Quarto Master", result.Name);
    }

    [Fact]
    public async Task GetRoomByIdAsync_Should_Return_Empty_Room_When_Not_Found()
    {
        // Arrange
        using var context = CreateContext();
        var repository = new RoomRepository(context);

        // Act
        var result = await repository.GetRoomByIdAsync(999);

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public async Task GetAllRoomsAsync_Should_Return_All_Rooms()
    {
        // Arrange
        using var context = CreateContext();
        context.Room.AddRange(
            CreateRoom(name: "Room 1"),
            CreateRoom(name: "Room 2"),
            CreateRoom(name: "Room 3")
        );
        await context.SaveChangesAsync();

        var repository = new RoomRepository(context);

        // Act
        var result = await repository.GetAllRoomsAsync();

        // Assert
        Assert.NotNull(result);
        Assert.Equal(3, result.Count());
    }

    [Fact]
    public async Task UpdateRoomAsync_Should_Return_True_And_Update_Room_When_Found()
    {
        // Arrange
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
            Price = new Domain.Room.ValueObjects.Price()
            {
                Value = 300,
                Currency = AcceptedCurrencies.Bitcoin
            }
        };

        // Act
        var result = await repository.UpdateRoomAsync(updatedRoom);

        // Assert
        Assert.True(result);

        var roomInDb = await context.Room.FindAsync(originalRoom.Id);
        Assert.NotNull(roomInDb);
        Assert.Equal("Quarto Premium", roomInDb.Name);
        Assert.Equal(3, roomInDb.Level);
        Assert.True(roomInDb.InMaintenance);
        Assert.Equal(updatedRoom.Price, roomInDb.Price);
    }

    [Fact]
    public async Task UpdateRoomAsync_Should_Return_False_When_Room_Not_Found()
    {
        // Arrange
        using var context = CreateContext();
        var repository = new RoomRepository(context);

        var room = new Room
        {
            Id = 999,
            Name = "Inexistente",
            Level = 2,
            InMaintenance = false,
            Price = new Domain.Room.ValueObjects.Price()
            {
                Value = 200,
                Currency = AcceptedCurrencies.Real
            }
        };

        // Act
        var result = await repository.UpdateRoomAsync(room);

        // Assert
        Assert.False(result);
    }

    [Fact]
    public async Task DeleteRoomAsync_Should_Return_True_And_Remove_Room_When_Found()
    {
        // Arrange
        using var context = CreateContext();
        var room = CreateRoom(name: "Quarto para excluir");
        context.Room.Add(room);
        await context.SaveChangesAsync();

        var repository = new RoomRepository(context);

        // Act
        var result = await repository.DeleteRoomAsync(room.Id);

        // Assert
        Assert.True(result);

        var deletedRoom = await context.Room.FindAsync(room.Id);
        Assert.Null(deletedRoom);
    }

    [Fact]
    public async Task DeleteRoomAsync_Should_Return_False_When_Room_Not_Found()
    {
        // Arrange
        using var context = CreateContext();
        var repository = new RoomRepository(context);

        // Act
        var result = await repository.DeleteRoomAsync(999);

        // Assert
        Assert.False(result);
    }
}