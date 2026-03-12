using Domain.Book.Entities;
using Domain.Book.Enums;
using Domain.Exceptions;
using Domain.Room.Entities;
using Domain.Room.Enums;
using Domain.Room.Ports;
using Domain.Room.ValueObjects;
using Moq;
using Xunit;

namespace DomainTests.Entities;

public class RoomTests
{
    private Room CreateValidRoom(int id = 0)
    {
        return new Room
        {
            Id = id,
            Name = "Luxury Room",
            Level = 1,
            InMaintenance = false,
            Price = new Price
            {
                Value = 150,
                Currency = AcceptedCurrencies.Dollars
            },
            Bookings = new List<Booking>()
        };
    }

    private Booking CreateBooking(
        int roomId,
        DateTime start,
        DateTime end,
        Status status = Status.Created)
    {
        var booking = new Booking
        {
            Id = 1,
            RoomId = roomId,
            GuestId = 1,
            PlacedAt = DateTime.UtcNow,
            Start = start,
            End = end
        };

        booking.SetStatus(status);

        return booking;
    }

    [Fact]
    public async Task Save_Should_Not_Throw_When_Room_Is_Valid_And_Id_Is_Zero()
    {
        var room = CreateValidRoom();

        var repositoryMock = new Mock<IRoomRepository>();
        repositoryMock
            .Setup(r => r.AddRoomAsync(It.IsAny<Room>()))
            .ReturnsAsync(1);

        var exception = await Record.ExceptionAsync(() => room.Save(repositoryMock.Object));

        Assert.Null(exception);
        Assert.Equal(1, room.Id);
        repositoryMock.Verify(r => r.AddRoomAsync(It.IsAny<Room>()), Times.Once);
    }

    [Fact]
    public async Task Save_Should_Call_UpdateRoomAsync_When_Id_Is_Greater_Than_Zero()
    {
        var room = CreateValidRoom(10);

        var repositoryMock = new Mock<IRoomRepository>();
        repositoryMock
            .Setup(r => r.UpdateRoomAsync(It.IsAny<Room>()))
            .ReturnsAsync(true);

        await room.Save(repositoryMock.Object);

        repositoryMock.Verify(r => r.UpdateRoomAsync(It.IsAny<Room>()), Times.Once);
        repositoryMock.Verify(r => r.AddRoomAsync(It.IsAny<Room>()), Times.Never);
    }

    [Fact]
    public async Task Save_Should_Throw_MissingRequiredInformationException_When_Name_Is_Empty()
    {
        var room = CreateValidRoom();
        room.Name = "";

        var repositoryMock = new Mock<IRoomRepository>();

        await Assert.ThrowsAsync<MissingRequiredInformationException>(() => room.Save(repositoryMock.Object));
    }

    [Fact]
    public async Task Save_Should_Throw_MissingRequiredInformationException_When_Name_Is_Too_Short()
    {
        var room = CreateValidRoom();
        room.Name = "AB";

        var repositoryMock = new Mock<IRoomRepository>();

        await Assert.ThrowsAsync<MissingRequiredInformationException>(() => room.Save(repositoryMock.Object));
    }

    [Fact]
    public async Task Save_Should_Throw_MissingRequiredInformationException_When_Name_Is_Too_Long()
    {
        var room = CreateValidRoom();
        room.Name = new string('A', 101);

        var repositoryMock = new Mock<IRoomRepository>();

        await Assert.ThrowsAsync<MissingRequiredInformationException>(() => room.Save(repositoryMock.Object));
    }

    [Fact]
    public async Task Save_Should_Throw_MissingRequiredInformationException_When_Level_Is_Zero()
    {
        var room = CreateValidRoom();
        room.Level = 0;

        var repositoryMock = new Mock<IRoomRepository>();

        await Assert.ThrowsAsync<MissingRequiredInformationException>(() => room.Save(repositoryMock.Object));
    }

    [Fact]
    public async Task Save_Should_Throw_MissingRequiredInformationException_When_Price_Is_Null()
    {
        var room = CreateValidRoom();
        room.Price = null!;

        var repositoryMock = new Mock<IRoomRepository>();

        await Assert.ThrowsAsync<MissingRequiredInformationException>(() => room.Save(repositoryMock.Object));
    }

    [Fact]
    public async Task Save_Should_Throw_MissingRequiredInformationException_When_Price_Value_Is_Zero()
    {
        var room = CreateValidRoom();
        room.Price.Value = 0;

        var repositoryMock = new Mock<IRoomRepository>();

        await Assert.ThrowsAsync<MissingRequiredInformationException>(() => room.Save(repositoryMock.Object));
    }

    [Fact]
    public async Task Save_Should_Throw_MissingRequiredInformationException_When_Price_Value_Is_Ten()
    {
        var room = CreateValidRoom();
        room.Price.Value = 10;

        var repositoryMock = new Mock<IRoomRepository>();

        await Assert.ThrowsAsync<MissingRequiredInformationException>(() => room.Save(repositoryMock.Object));
    }

    [Fact]
    public async Task Save_Should_Throw_MissingRequiredInformationException_When_Currency_Is_Invalid()
    {
        var room = CreateValidRoom();
        room.Price.Currency = (AcceptedCurrencies)999;

        var repositoryMock = new Mock<IRoomRepository>();

        await Assert.ThrowsAsync<MissingRequiredInformationException>(() => room.Save(repositoryMock.Object));
    }

    [Fact]
    public void HasGuest_Should_Return_False_When_Bookings_Is_Null()
    {
        var room = CreateValidRoom();
        room.Bookings = null!;

        Assert.False(room.HasGuest);
    }

    [Fact]
    public void HasGuest_Should_Return_False_When_Bookings_Is_Empty()
    {
        var room = CreateValidRoom();
        room.Bookings = new List<Booking>();

        Assert.False(room.HasGuest);
    }

    [Fact]
    public void HasGuest_Should_Return_False_When_Booking_Is_In_The_Future()
    {
        var room = CreateValidRoom(2);

        room.Bookings = new List<Booking>
        {
            CreateBooking(
                room.Id,
                DateTime.UtcNow.AddHours(1),
                DateTime.UtcNow.AddHours(3),
                Status.Created)
        };

        Assert.False(room.HasGuest);
    }

    [Fact]
    public void HasGuest_Should_Return_False_When_Booking_Is_In_The_Past()
    {
        var room = CreateValidRoom(2);

        room.Bookings = new List<Booking>
        {
            CreateBooking(
                room.Id,
                DateTime.UtcNow.AddHours(-5),
                DateTime.UtcNow.AddHours(-1),
                Status.Created)
        };

        Assert.False(room.HasGuest);
    }

    [Fact]
    public void HasGuest_Should_Return_True_When_There_Is_Active_Booking_Now_With_Created_Status()
    {
        var room = CreateValidRoom(2);

        room.Bookings = new List<Booking>
        {
            CreateBooking(
                room.Id,
                DateTime.UtcNow.AddHours(-1),
                DateTime.UtcNow.AddHours(2),
                Status.Created)
        };

        Assert.True(room.HasGuest);
    }

    [Fact]
    public void HasGuest_Should_Return_True_When_There_Is_Active_Booking_Now_With_Paid_Status()
    {
        var room = CreateValidRoom(2);

        room.Bookings = new List<Booking>
        {
            CreateBooking(
                room.Id,
                DateTime.UtcNow.AddHours(-1),
                DateTime.UtcNow.AddHours(2),
                Status.Paid)
        };

        Assert.True(room.HasGuest);
    }

    [Fact]
    public void HasGuest_Should_Return_False_When_Booking_Status_Does_Not_Occupy_Room()
    {
        var room = CreateValidRoom(2);

        room.Bookings = new List<Booking>
        {
            CreateBooking(
                room.Id,
                DateTime.UtcNow.AddHours(-1),
                DateTime.UtcNow.AddHours(2),
                Status.Canceled)
        };

        Assert.False(room.HasGuest);
    }

    [Fact]
    public void HasGuest_Should_Return_False_When_Booking_Belongs_To_Another_Room()
    {
        var room = CreateValidRoom(2);

        room.Bookings = new List<Booking>
        {
            CreateBooking(
                999,
                DateTime.UtcNow.AddHours(-1),
                DateTime.UtcNow.AddHours(2),
                Status.Created)
        };

        Assert.False(room.HasGuest);
    }

    [Fact]
    public void IsAvaliable_Should_Return_False_When_InMaintenance_Is_True()
    {
        var room = CreateValidRoom();
        room.InMaintenance = true;

        Assert.False(room.IsAvaliable);
    }

    [Fact]
    public void IsAvaliable_Should_Return_False_When_HasGuest_Is_True()
    {
        var room = CreateValidRoom(2);
        room.InMaintenance = false;
        room.Bookings = new List<Booking>
        {
            CreateBooking(
                room.Id,
                DateTime.UtcNow.AddHours(-1),
                DateTime.UtcNow.AddHours(1),
                Status.Created)
        };

        Assert.False(room.IsAvaliable);
    }

    [Fact]
    public void IsAvaliable_Should_Return_True_When_Not_InMaintenance_And_HasNoGuest()
    {
        var room = CreateValidRoom(2);
        room.InMaintenance = false;
        room.Bookings = new List<Booking>
        {
            CreateBooking(
                room.Id,
                DateTime.UtcNow.AddHours(2),
                DateTime.UtcNow.AddHours(4),
                Status.Created)
        };

        Assert.True(room.IsAvaliable);
    }
}