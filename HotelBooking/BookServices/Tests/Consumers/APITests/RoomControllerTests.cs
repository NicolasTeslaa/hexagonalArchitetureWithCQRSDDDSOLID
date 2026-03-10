using API.Controllers;
using Application;
using Application.Room.DTO;
using Application.Room.Port;
using Application.Room.Request;
using Application.Room.Responses;
using Domain.Exceptions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.TestPlatform.CommunicationUtilities;
using Moq;
using Xunit;

namespace API.Tests.Controllers;

public class RoomControllerTests
{
    private readonly Mock<ILogger<RoomController>> _loggerMock;
    private readonly Mock<IRoomManager> _serviceMock;
    private readonly RoomController _controller;

    public RoomControllerTests()
    {
        _loggerMock = new Mock<ILogger<RoomController>>();
        _serviceMock = new Mock<IRoomManager>();
        _controller = new RoomController(_loggerMock.Object, _serviceMock.Object);
    }

    private static RoomDTO CreateRoomDTO(int id = 1)
    {
        return new RoomDTO
        {
            Id = id,
            Name = "Room 101",
            Level = 1,
            InMaintenance = false
        };
    }

    [Fact]
    public async Task GetAll_Should_Return_Ok_With_Rooms()
    {
        var rooms = new List<RoomDTO>
        {
            CreateRoomDTO(1),
            CreateRoomDTO(2)
        };

        _serviceMock
            .Setup(s => s.GetAllRoomsAsync())
            .ReturnsAsync(rooms);

        var result = await _controller.GetAll();

        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var returnedRooms = Assert.IsAssignableFrom<IEnumerable<RoomDTO>>(okResult.Value);
        Assert.Equal(2, returnedRooms.Count());
    }

    [Fact]
    public async Task GetAll_Should_Return_500_When_Exception_Is_Thrown()
    {
        _serviceMock
            .Setup(s => s.GetAllRoomsAsync())
            .ThrowsAsync(new Exception("error"));

        var result = await _controller.GetAll();

        var objectResult = Assert.IsType<ObjectResult>(result.Result);
        Assert.Equal(StatusCodes.Status500InternalServerError, objectResult.StatusCode);
    }

    [Fact]
    public async Task GetById_Should_Return_Ok_When_Room_Exists()
    {
        var response = new RoomResponse
        {
            Success = true,
            Data = CreateRoomDTO(10)
        };

        _serviceMock
            .Setup(s => s.GetRoomByIdAsync(10))
            .ReturnsAsync(response);

        var result = await _controller.GetById(10);

        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var returnedRoom = Assert.IsType<RoomDTO>(okResult.Value);

        Assert.Equal(10, returnedRoom.Id);
        Assert.Equal("Room 101", returnedRoom.Name);
    }

    [Fact]
    public async Task GetById_Should_Return_NotFound_When_Room_Does_Not_Exist()
    {
        var response = new RoomResponse
        {
            Success = false,
            ErrorCode = ErrorCodes.NOT_FOUND,
            Message = "Room not found."
        };

        _serviceMock
            .Setup(s => s.GetRoomByIdAsync(10))
            .ReturnsAsync(response);

        var result = await _controller.GetById(10);

        var notFoundResult = Assert.IsType<NotFoundObjectResult>(result.Result);
        var returnedResponse = Assert.IsType<RoomResponse>(notFoundResult.Value);

        Assert.False(returnedResponse.Success);
        Assert.Equal(ErrorCodes.NOT_FOUND, returnedResponse.ErrorCode);
        Assert.Equal("Room not found.", returnedResponse.Message);
    }

    [Fact]
    public async Task GetById_Should_Return_500_When_Response_Is_NotFound_Nor_Success()
    {
        var response = new RoomResponse
        {
            Success = false,
            ErrorCode = ErrorCodes.MISSING_REQUIRED_INFORMATION,
            Message = "Invalid request."
        };

        _serviceMock
            .Setup(s => s.GetRoomByIdAsync(10))
            .ReturnsAsync(response);

        var result = await _controller.GetById(10);

        var objectResult = Assert.IsType<ObjectResult>(result.Result);
        Assert.Equal(StatusCodes.Status500InternalServerError, objectResult.StatusCode);

        var returnedResponse = Assert.IsType<RoomResponse>(objectResult.Value);
        Assert.False(returnedResponse.Success);
        Assert.Equal(ErrorCodes.MISSING_REQUIRED_INFORMATION, returnedResponse.ErrorCode);
        Assert.Equal("Invalid request.", returnedResponse.Message);
    }

    [Fact]
    public async Task GetById_Should_Return_500_When_Exception_Is_Thrown()
    {
        _serviceMock
            .Setup(s => s.GetRoomByIdAsync(10))
            .ThrowsAsync(new Exception("error"));

        var result = await _controller.GetById(10);

        var objectResult = Assert.IsType<ObjectResult>(result.Result);
        Assert.Equal(StatusCodes.Status500InternalServerError, objectResult.StatusCode);

        var response = Assert.IsType<RoomResponse>(objectResult.Value);
        Assert.False(response.Success);
        Assert.Equal(ErrorCodes.UNEXPECTED_ERROR, response.ErrorCode);
    }

    [Fact]
    public async Task Create_Should_Return_CreatedAtAction_When_Success()
    {
        var room = CreateRoomDTO(0);

        var response = new RoomResponse
        {
            Success = true,
            Data = CreateRoomDTO(20)
        };

        _serviceMock
            .Setup(s => s.AddRoomAsync(It.IsAny<CreateRoomRequest>()))
            .ReturnsAsync(response);

        var result = await _controller.Create(room);

        var createdResult = Assert.IsType<CreatedAtActionResult>(result.Result);
        Assert.Equal(nameof(RoomController.GetById), createdResult.ActionName);

        var returnedResponse = Assert.IsType<RoomResponse>(createdResult.Value);
        Assert.True(returnedResponse.Success);
        Assert.Equal(20, returnedResponse.Data.Id);
    }

    [Fact]
    public async Task Create_Should_Return_BadRequest_When_MissingRequiredInformation()
    {
        var room = CreateRoomDTO(0);

        var response = new RoomResponse
        {
            Success = false,
            ErrorCode = ErrorCodes.MISSING_REQUIRED_INFORMATION,
            Message = "Missing required information."
        };

        _serviceMock
            .Setup(s => s.AddRoomAsync(It.IsAny<CreateRoomRequest>()))
            .ReturnsAsync(response);

        var result = await _controller.Create(room);

        var badRequestResult = Assert.IsType<BadRequestObjectResult>(result.Result);
        var returnedResponse = Assert.IsType<RoomResponse>(badRequestResult.Value);

        Assert.False(returnedResponse.Success);
        Assert.Equal(ErrorCodes.MISSING_REQUIRED_INFORMATION, returnedResponse.ErrorCode);
    }

    [Fact]
    public async Task Create_Should_Return_500_When_CouldNotStoreData()
    {
        var room = CreateRoomDTO(0);

        var response = new RoomResponse
        {
            Success = false,
            ErrorCode = ErrorCodes.COULD_NOT_STORE_DATA,
            Message = "Could not store data."
        };

        _serviceMock
            .Setup(s => s.AddRoomAsync(It.IsAny<CreateRoomRequest>()))
            .ReturnsAsync(response);

        var result = await _controller.Create(room);

        var objectResult = Assert.IsType<ObjectResult>(result.Result);
        Assert.Equal(StatusCodes.Status500InternalServerError, objectResult.StatusCode);

        var returnedResponse = Assert.IsType<RoomResponse>(objectResult.Value);
        Assert.Equal(ErrorCodes.COULD_NOT_STORE_DATA, returnedResponse.ErrorCode);
    }

    [Fact]
    public async Task Create_Should_Return_500_When_Exception_Is_Thrown()
    {
        var room = CreateRoomDTO(0);

        _serviceMock
            .Setup(s => s.AddRoomAsync(It.IsAny<CreateRoomRequest>()))
            .ThrowsAsync(new Exception("error"));

        var result = await _controller.Create(room);

        var objectResult = Assert.IsType<ObjectResult>(result.Result);
        Assert.Equal(StatusCodes.Status500InternalServerError, objectResult.StatusCode);

        var returnedResponse = Assert.IsType<RoomResponse>(objectResult.Value);
        Assert.False(returnedResponse.Success);
        Assert.Equal(ErrorCodes.UNEXPECTED_ERROR, returnedResponse.ErrorCode);
    }

    [Fact]
    public async Task Update_Should_Return_BadRequest_When_Id_Is_Different_From_RoomId()
    {
        var room = CreateRoomDTO(2);

        var result = await _controller.Update(1, room);

        var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
        var response = Assert.IsType<RoomResponse>(badRequestResult.Value);

        Assert.False(response.Success);
        Assert.Equal(ErrorCodes.MISSING_REQUIRED_INFORMATION, response.ErrorCode);
        Assert.Equal("Route id is different from room id.", response.Message);
    }

    [Fact]
    public async Task Update_Should_Return_Ok_When_Update_Succeeds()
    {
        var room = CreateRoomDTO(1);

        _serviceMock
            .Setup(s => s.UpdateRoomAsync(room))
            .ReturnsAsync(true);

        var result = await _controller.Update(1, room);

        var okResult = Assert.IsType<OkObjectResult>(result);
        var response = Assert.IsType<RoomResponse>(okResult.Value);

        Assert.True(response.Success);
        Assert.Equal(room.Id, response.Data.Id);
    }

    [Fact]
    public async Task Update_Should_Return_BadRequest_When_Update_Returns_False()
    {
        var room = CreateRoomDTO(1);

        _serviceMock
            .Setup(s => s.UpdateRoomAsync(room))
            .ReturnsAsync(false);

        var result = await _controller.Update(1, room);

        var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
        var response = Assert.IsType<RoomResponse>(badRequestResult.Value);

        Assert.False(response.Success);
        Assert.Equal(ErrorCodes.COULD_NOT_STORE_DATA, response.ErrorCode);
        Assert.Equal("Could not update room.", response.Message);
    }

    [Fact]
    public async Task Update_Should_Return_BadRequest_When_MissingRequiredInformationException_Is_Thrown()
    {
        var room = CreateRoomDTO(1);

        _serviceMock
            .Setup(s => s.UpdateRoomAsync(room))
            .ThrowsAsync(new MissingRequiredInformationException());

        var result = await _controller.Update(1, room);

        var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
        var response = Assert.IsType<RoomResponse>(badRequestResult.Value);

        Assert.False(response.Success);
        Assert.Equal(ErrorCodes.MISSING_REQUIRED_INFORMATION, response.ErrorCode);
        Assert.Equal("Missing required information.", response.Message);
    }

    [Fact]
    public async Task Update_Should_Return_500_When_Unexpected_Exception_Is_Thrown()
    {
        var room = CreateRoomDTO(1);

        _serviceMock
            .Setup(s => s.UpdateRoomAsync(room))
            .ThrowsAsync(new Exception("error"));

        var result = await _controller.Update(1, room);

        var objectResult = Assert.IsType<ObjectResult>(result);
        Assert.Equal(StatusCodes.Status500InternalServerError, objectResult.StatusCode);

        var response = Assert.IsType<RoomResponse>(objectResult.Value);
        Assert.False(response.Success);
        Assert.Equal(ErrorCodes.UNEXPECTED_ERROR, response.ErrorCode);
    }

    [Fact]
    public async Task Delete_Should_Return_NoContent_When_Delete_Succeeds()
    {
        _serviceMock
            .Setup(s => s.DeleteRoomAsync(1))
            .ReturnsAsync(true);

        var result = await _controller.Delete(1);

        Assert.IsType<NoContentResult>(result);
    }

    [Fact]
    public async Task Delete_Should_Return_NotFound_When_Room_Does_Not_Exist()
    {
        _serviceMock
            .Setup(s => s.DeleteRoomAsync(1))
            .ReturnsAsync(false);

        var result = await _controller.Delete(1);

        var notFoundResult = Assert.IsType<NotFoundObjectResult>(result);
        var response = Assert.IsType<RoomResponse>(notFoundResult.Value);

        Assert.False(response.Success);
        Assert.Equal(ErrorCodes.NOT_FOUND, response.ErrorCode);
        Assert.Equal("Room not found.", response.Message);
    }

    [Fact]
    public async Task Delete_Should_Return_500_When_Exception_Is_Thrown()
    {
        _serviceMock
            .Setup(s => s.DeleteRoomAsync(1))
            .ThrowsAsync(new Exception("error"));

        var result = await _controller.Delete(1);

        var objectResult = Assert.IsType<ObjectResult>(result);
        Assert.Equal(StatusCodes.Status500InternalServerError, objectResult.StatusCode);

        var response = Assert.IsType<RoomResponse>(objectResult.Value);
        Assert.False(response.Success);
        Assert.Equal(ErrorCodes.UNEXPECTED_ERROR, response.ErrorCode);
    }
}