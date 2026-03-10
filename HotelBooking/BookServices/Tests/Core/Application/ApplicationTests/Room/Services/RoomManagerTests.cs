using Application;
using Application.Room.DTO;
using Application.Room.Request;
using Application.Room.Services;
using Domain.Exceptions;
using Domain.Room.Enums;
using Domain.Room.Ports;
using Domain.Room.ValueObjects;
using Moq;
using RoomEntity = Domain.Room.Entities.Room;

namespace ApplicationTests.Room.Services;

public class RoomManagerTests
{
    private readonly Mock<IRoomRepository> _roomRepositoryMock;
    private readonly RoomManager _roomManager;

    public RoomManagerTests()
    {
        _roomRepositoryMock = new Mock<IRoomRepository>();
        _roomManager = new RoomManager(_roomRepositoryMock.Object);
    }

    private static RoomDTO CreateValidRoomDTO(int id = 0)
    {
        return new RoomDTO
        {
            Id = id,
            Name = "Room 101",
            Level = 1,
            InMaintenance = false,
            Price = new Price
            {
                Value = 150,
                Currency = AcceptedCurrencies.Real
            }
        };
    }

    private static RoomEntity CreateValidRoomEntity(int id = 1)
    {
        return new RoomEntity
        {
            Id = id,
            Name = "Room 101",
            Level = 1,
            InMaintenance = false,
            Price = new Price
            {
                Value = 150,
                Currency = AcceptedCurrencies.Real
            }
        };
    }

    [Fact]
    public async Task AddRoomAsync_Should_Return_Success_When_Room_Is_Valid()
    {
        var request = new CreateRoomRequest
        {
            Data = CreateValidRoomDTO()
        };

        _roomRepositoryMock
            .Setup(r => r.AddRoomAsync(It.IsAny<RoomEntity>()))
            .ReturnsAsync(10);

        var result = await _roomManager.AddRoomAsync(request);

        Assert.True(result.Success);
        Assert.NotNull(result.Data);
        Assert.Equal("Room 101", result.Data.Name);

        _roomRepositoryMock.Verify(r => r.AddRoomAsync(It.IsAny<RoomEntity>()), Times.Once);
    }

    [Fact]
    public async Task AddRoomAsync_Should_Return_MissingRequiredInformation_When_Room_Is_Invalid()
    {
        var request = new CreateRoomRequest
        {
            Data = new RoomDTO
            {
                Id = 0,
                Name = "",
                Level = 0,
                InMaintenance = false,
                Price = null
            }
        };

        var result = await _roomManager.AddRoomAsync(request);

        Assert.False(result.Success);
        Assert.Equal(ErrorCodes.MISSING_REQUIRED_INFORMATION, result.ErrorCode);
        Assert.Equal("Missing required information.", result.Message);

        _roomRepositoryMock.Verify(r => r.AddRoomAsync(It.IsAny<RoomEntity>()), Times.Never);
    }

    [Fact]
    public async Task AddRoomAsync_Should_Return_UnexpectedError_When_Exception_Is_Thrown()
    {
        var request = new CreateRoomRequest
        {
            Data = CreateValidRoomDTO()
        };

        _roomRepositoryMock
            .Setup(r => r.AddRoomAsync(It.IsAny<RoomEntity>()))
            .ThrowsAsync(new Exception("database error"));

        var result = await _roomManager.AddRoomAsync(request);

        Assert.False(result.Success);
        Assert.Equal(ErrorCodes.UNEXPECTED_ERROR, result.ErrorCode);
    }

    [Fact]
    public async Task DeleteRoomAsync_Should_Return_True_When_Repository_Returns_True()
    {
        _roomRepositoryMock
            .Setup(r => r.DeleteRoomAsync(1))
            .ReturnsAsync(true);

        var result = await _roomManager.DeleteRoomAsync(1);

        Assert.True(result);
    }

    [Fact]
    public async Task DeleteRoomAsync_Should_Return_False_When_Repository_Returns_False()
    {
        _roomRepositoryMock
            .Setup(r => r.DeleteRoomAsync(99))
            .ReturnsAsync(false);

        var result = await _roomManager.DeleteRoomAsync(99);

        Assert.False(result);
    }

    [Fact]
    public async Task GetAllRoomsAsync_Should_Return_Mapped_RoomDTO_List()
    {
        var rooms = new List<RoomEntity>
        {
            CreateValidRoomEntity(1),
            CreateValidRoomEntity(2)
        };

        _roomRepositoryMock
            .Setup(r => r.GetAllRoomsAsync())
            .ReturnsAsync(rooms);

        var result = await _roomManager.GetAllRoomsAsync();

        Assert.NotNull(result);
        Assert.Equal(2, result.Count());

        Assert.All(result, room =>
        {
            Assert.IsType<RoomDTO>(room);
            Assert.Equal("Room 101", room.Name);
        });
    }

    [Fact]
    public async Task GetRoomByIdAsync_Should_Return_Success_When_Room_Exists()
    {
        var room = CreateValidRoomEntity(5);

        _roomRepositoryMock
            .Setup(r => r.GetRoomByIdAsync(5))
            .ReturnsAsync(room);

        var result = await _roomManager.GetRoomByIdAsync(5);

        Assert.True(result.Success);
        Assert.NotNull(result.Data);
        Assert.Equal(5, result.Data.Id);
        Assert.Equal("Room 101", result.Data.Name);
        Assert.Equal(1, result.Data.Level);
        Assert.False(result.Data.InMaintenance);
        Assert.NotNull(result.Data.Price);
    }

    [Fact]
    public async Task GetRoomByIdAsync_Should_Return_NotFound_When_Room_Does_Not_Exist()
    {
        _roomRepositoryMock
            .Setup(r => r.GetRoomByIdAsync(5))
            .ReturnsAsync((RoomEntity?)null);

        var result = await _roomManager.GetRoomByIdAsync(5);

        Assert.False(result.Success);
        Assert.Equal(ErrorCodes.NOT_FOUND, result.ErrorCode);
        Assert.Equal("Room not found", result.Message);
        Assert.Null(result.Data);
    }

    [Fact]
    public async Task UpdateRoomAsync_Should_Return_True_When_Update_Succeeds()
    {
        var roomDto = CreateValidRoomDTO(1);

        _roomRepositoryMock
            .Setup(r => r.UpdateRoomAsync(It.IsAny<RoomEntity>()))
            .ReturnsAsync(true);

        var result = await _roomManager.UpdateRoomAsync(roomDto);

        Assert.True(result);
        _roomRepositoryMock.Verify(r => r.UpdateRoomAsync(It.IsAny<RoomEntity>()), Times.Once);
    }

    [Fact]
    public async Task UpdateRoomAsync_Should_Throw_MissingRequiredInformationException_When_Room_Is_Invalid()
    {
        var roomDto = new RoomDTO
        {
            Id = 1,
            Name = "",
            Level = 0,
            InMaintenance = false,
            Price = null
        };

        await Assert.ThrowsAsync<MissingRequiredInformationException>(() =>
            _roomManager.UpdateRoomAsync(roomDto));

        _roomRepositoryMock.Verify(r => r.UpdateRoomAsync(It.IsAny<RoomEntity>()), Times.Never);
    }

    [Fact]
    public async Task UpdateRoomAsync_Should_Return_False_When_Add_Returns_Zero_For_New_Room()
    {
        var roomDto = CreateValidRoomDTO(0);

        _roomRepositoryMock
            .Setup(r => r.AddRoomAsync(It.IsAny<RoomEntity>()))
            .ReturnsAsync(0);

        var result = await _roomManager.UpdateRoomAsync(roomDto);

        Assert.False(result);
    }
}