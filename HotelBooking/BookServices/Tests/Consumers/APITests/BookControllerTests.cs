using API.Controllers;
using Application;
using Application.Book.Commands;
using Application.Book.DTO;
using Application.Book.Port;
using Application.Book.Responses;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;

namespace API.Tests.Controllers;

public class BookControllerTests
{
    private readonly Mock<IBookManager> _serviceMock;
    private readonly Mock<ILogger<BookController>> _loggerMock;
    private readonly Mock<IMediator> _mediatorMock;
    private readonly BookController _controller;

    public BookControllerTests()
    {
        _serviceMock = new Mock<IBookManager>();
        _loggerMock = new Mock<ILogger<BookController>>();
        _mediatorMock = new Mock<IMediator>();

        _controller = new BookController(
            _serviceMock.Object,
            _loggerMock.Object,
            _mediatorMock.Object);
    }

    private static BookDTO CreateBookDto()
    {
        return new BookDTO
        {
            Id = 1,
            PlacedAt = DateTime.UtcNow,
            Start = DateTime.UtcNow.AddHours(1),
            End = DateTime.UtcNow.AddHours(2),
            RoomId = 2,
            GuestId = 3
        };
    }

    [Fact]
    public async Task GetBookById_Should_Return_Ok_When_Mediator_Returns_Success()
    {
        var dto = CreateBookDto();

        _mediatorMock
            .Setup(m => m.Send(
                It.Is<GetBookingQuery>(q => q.Id == 1),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(new BookResponse
            {
                Success = true,
                Data = dto
            });

        var result = await _controller.GetBookById(1);

        var okResult = Assert.IsType<OkObjectResult>(result);
        var returnedDto = Assert.IsType<BookDTO>(okResult.Value);
        Assert.Equal(dto.Id, returnedDto.Id);
    }

    [Fact]
    public async Task GetBookById_Should_Return_NotFound_When_Mediator_Returns_NotFound()
    {
        _mediatorMock
            .Setup(m => m.Send(
                It.Is<GetBookingQuery>(q => q.Id == 1),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(new BookResponse
            {
                Success = false,
                ErrorCode = ErrorCodes.NOT_FOUND,
                Message = "Book not found."
            });

        var result = await _controller.GetBookById(1);

        var notFoundResult = Assert.IsType<NotFoundObjectResult>(result);
        var response = Assert.IsType<BookResponse>(notFoundResult.Value);
        Assert.False(response.Success);
        Assert.Equal(ErrorCodes.NOT_FOUND, response.ErrorCode);
    }

    [Fact]
    public async Task GetBookById_Should_Return_InternalServerError_When_Mediator_Returns_Unexpected_Error()
    {
        _mediatorMock
            .Setup(m => m.Send(
                It.Is<GetBookingQuery>(q => q.Id == 1),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(new BookResponse
            {
                Success = false,
                ErrorCode = ErrorCodes.UNEXPECTED_ERROR,
                Message = "An unexpected error occurred."
            });

        var result = await _controller.GetBookById(1);

        var objectResult = Assert.IsType<ObjectResult>(result);
        Assert.Equal(StatusCodes.Status500InternalServerError, objectResult.StatusCode);

        var response = Assert.IsType<BookResponse>(objectResult.Value);
        Assert.False(response.Success);
        Assert.Equal(ErrorCodes.UNEXPECTED_ERROR, response.ErrorCode);
    }

    [Fact]
    public async Task GetBookById_Should_Return_InternalServerError_When_Mediator_Throws_Exception()
    {
        _mediatorMock
            .Setup(m => m.Send(
                It.Is<GetBookingQuery>(q => q.Id == 1),
                It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception("erro"));

        var result = await _controller.GetBookById(1);

        var objectResult = Assert.IsType<ObjectResult>(result);
        Assert.Equal(StatusCodes.Status500InternalServerError, objectResult.StatusCode);
        Assert.NotNull(objectResult.Value);
    }

    [Fact]
    public async Task CreateBook_Should_Return_Ok_When_Mediator_Returns_Success()
    {
        var dto = CreateBookDto();

        _mediatorMock
            .Setup(m => m.Send(
                It.Is<CreateBookingCommand>(c => c.BookingDTO == dto),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(new BookResponse
            {
                Success = true,
                Data = dto
            });

        var result = await _controller.CreateBook(dto);

        var okResult = Assert.IsType<OkObjectResult>(result);
        var returnedDto = Assert.IsType<BookDTO>(okResult.Value);
        Assert.Equal(dto.Id, returnedDto.Id);
    }

    [Theory]
    [InlineData(ErrorCodes.ROOM_REQUIRED)]
    [InlineData(ErrorCodes.GUEST_REQUIRED)]
    [InlineData(ErrorCodes.INVALID_BOOKING_DATE_RANGE)]
    [InlineData(ErrorCodes.BOOKING_IN_PAST)]
    public async Task CreateBook_Should_Return_BadRequest_For_Validation_Errors(ErrorCodes errorCode)
    {
        var dto = CreateBookDto();

        _mediatorMock
            .Setup(m => m.Send(
                It.Is<CreateBookingCommand>(c => c.BookingDTO == dto),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(new BookResponse
            {
                Success = false,
                ErrorCode = errorCode,
                Message = "validation error"
            });

        var result = await _controller.CreateBook(dto);

        var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
        var response = Assert.IsType<BookResponse>(badRequestResult.Value);
        Assert.False(response.Success);
        Assert.Equal(errorCode, response.ErrorCode);
    }

    [Theory]
    [InlineData(ErrorCodes.ROOM_UNAVAILABLE)]
    [InlineData(ErrorCodes.BOOKING_CONFLICT)]
    public async Task CreateBook_Should_Return_Conflict_For_Conflict_Errors(ErrorCodes errorCode)
    {
        var dto = CreateBookDto();

        _mediatorMock
            .Setup(m => m.Send(
                It.Is<CreateBookingCommand>(c => c.BookingDTO == dto),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(new BookResponse
            {
                Success = false,
                ErrorCode = errorCode,
                Message = "conflict"
            });

        var result = await _controller.CreateBook(dto);

        var conflictResult = Assert.IsType<ConflictObjectResult>(result);
        var response = Assert.IsType<BookResponse>(conflictResult.Value);
        Assert.False(response.Success);
        Assert.Equal(errorCode, response.ErrorCode);
    }

    [Fact]
    public async Task CreateBook_Should_Return_NotFound_When_Mediator_Returns_NotFound()
    {
        var dto = CreateBookDto();

        _mediatorMock
            .Setup(m => m.Send(
                It.Is<CreateBookingCommand>(c => c.BookingDTO == dto),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(new BookResponse
            {
                Success = false,
                ErrorCode = ErrorCodes.NOT_FOUND,
                Message = "not found"
            });

        var result = await _controller.CreateBook(dto);

        var notFoundResult = Assert.IsType<NotFoundObjectResult>(result);
        var response = Assert.IsType<BookResponse>(notFoundResult.Value);
        Assert.False(response.Success);
        Assert.Equal(ErrorCodes.NOT_FOUND, response.ErrorCode);
    }

    [Fact]
    public async Task CreateBook_Should_Return_InternalServerError_When_Mediator_Returns_Unexpected_Error()
    {
        var dto = CreateBookDto();

        _mediatorMock
            .Setup(m => m.Send(
                It.Is<CreateBookingCommand>(c => c.BookingDTO == dto),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(new BookResponse
            {
                Success = false,
                ErrorCode = ErrorCodes.UNEXPECTED_ERROR,
                Message = "unexpected"
            });

        var result = await _controller.CreateBook(dto);

        var objectResult = Assert.IsType<ObjectResult>(result);
        Assert.Equal(StatusCodes.Status500InternalServerError, objectResult.StatusCode);

        var response = Assert.IsType<BookResponse>(objectResult.Value);
        Assert.False(response.Success);
        Assert.Equal(ErrorCodes.UNEXPECTED_ERROR, response.ErrorCode);
    }

    [Fact]
    public async Task CreateBook_Should_Return_InternalServerError_When_Mediator_Throws_Exception()
    {
        var dto = CreateBookDto();

        _mediatorMock
            .Setup(m => m.Send(
                It.Is<CreateBookingCommand>(c => c.BookingDTO == dto),
                It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception("erro"));

        var result = await _controller.CreateBook(dto);

        var objectResult = Assert.IsType<ObjectResult>(result);
        Assert.Equal(StatusCodes.Status500InternalServerError, objectResult.StatusCode);
        Assert.NotNull(objectResult.Value);
    }
}