using Domain.Entities;
using Domain.Enums;
using Domain.Exceptions;
using Domain.Ports;
using Domain.ValueObjects;
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
            }
        };
    }

    [Fact]
    public async Task Save_Should_Not_Throw_When_Room_Is_Valid_And_Id_Is_Zero()
    {
        // Arrange
        var room = CreateValidRoom();

        var repositoryMock = new Mock<IRoomRepository>();
        repositoryMock
            .Setup(r => r.AddRoomAsync(It.IsAny<Room>()))
            .ReturnsAsync(1);

        // Act
        var exception = await Record.ExceptionAsync(() => room.Save(repositoryMock.Object));

        // Assert
        Assert.Null(exception);
        Assert.Equal(1, room.Id);
        repositoryMock.Verify(r => r.AddRoomAsync(It.IsAny<Room>()), Times.Once);
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
    public async Task Save_Should_Throw_MissingRequiredInformationException_When_Currency_Is_Invalid()
    {
        var room = CreateValidRoom();
        room.Price.Currency = (AcceptedCurrencies)999;

        var repositoryMock = new Mock<IRoomRepository>();

        await Assert.ThrowsAsync<MissingRequiredInformationException>(() => room.Save(repositoryMock.Object));
    }

    [Fact]
    public async Task Save_Should_Call_UpdateRoomAsync_When_Id_Is_Greater_Than_Zero()
    {
        // Arrange
        var room = CreateValidRoom(10);

        var repositoryMock = new Mock<IRoomRepository>();
        repositoryMock
            .Setup(r => r.UpdateRoomAsync(It.IsAny<Room>()))
            .ReturnsAsync(true);

        // Act
        await room.Save(repositoryMock.Object);

        // Assert
        repositoryMock.Verify(r => r.UpdateRoomAsync(It.IsAny<Room>()), Times.Once);
        repositoryMock.Verify(r => r.AddRoomAsync(It.IsAny<Room>()), Times.Never);
    }

    [Fact]
    public void IsAvaliable_Should_Return_False_When_InMaintenance_Is_True()
    {
        // Arrange
        var room = CreateValidRoom();
        room.InMaintenance = true;

        // Act / Assert
        Assert.False(room.IsAvaliable);
    }

    [Fact]
    public void IsAvaliable_Should_Always_Return_False_Because_HasGuest_Is_Always_True()
    {
        // Arrange
        var room = CreateValidRoom();
        room.InMaintenance = false;

        // Act / Assert
        Assert.False(room.IsAvaliable);
    }
}