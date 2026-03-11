using Application;
using Application.Book.DTO;
using Application.Book.Services;
using Domain.Book.Entities;
using Domain.Book.Enums;
using Domain.Book.Ports;
using Domain.Room.Entities;
using Domain.Room.Enums;
using Domain.Room.ValueObjects;
using Moq;
using Action = Domain.Book.Enums.Action;

namespace ApplicationTests.Book.Services;

public class BookManagerTests
{
    private readonly Mock<IBookRepository> _bookRepositoryMock;
    private readonly BookManager _bookManager;

    public BookManagerTests()
    {
        _bookRepositoryMock = new Mock<IBookRepository>();
        _bookManager = new BookManager(_bookRepositoryMock.Object);
    }

    private static BookDTO CreateValidDto()
    {
        return new BookDTO
        {
            Id = 0,
            Start = DateTime.UtcNow.AddHours(2),
            End = DateTime.UtcNow.AddHours(4),
            RoomId = 1,
            GuestId = 1
        };
    }

    private static Domain.Room.Entities.Room CreateValidRoom(bool inMaintenance = false)
    {
        return new Domain.Room.Entities.Room
        {
            Id = 1,
            Name = "Luxury Room",
            Level = 1,
            InMaintenance = inMaintenance,
            Price = new Price
            {
                Value = 150,
                Currency = AcceptedCurrencies.Dollars
            },
            Bookings = new List<Booking>()
        };
    }

    private static Domain.Guest.Entities.Guest CreateValidGuest()
    {
        return new Domain.Guest.Entities.Guest
        {
            Id = 1,
            Name = "Tesla",
            Email = "tesla@gmail.com",
            DocumentId = new Domain.Guest.ValueObjects.PersonId
            {
                IdNumber = "12345678901",
                Document = Domain.Guest.ValueObjects.DocumentType.DriverLicence
            }
        };
    }

    [Fact]
    public async Task CreateBookAsync_Should_Return_Success_When_Booking_Is_Valid()
    {
        var dto = CreateValidDto();
        var room = CreateValidRoom();
        var guest = CreateValidGuest();

        _bookRepositoryMock
            .Setup(r => r.GetRoomAndGuestAsync(dto.RoomId, dto.GuestId))
            .ReturnsAsync((room, guest));

        _bookRepositoryMock
            .Setup(r => r.HasConflictAsync(dto.RoomId, dto.Start, dto.End, null))
            .ReturnsAsync(false);

        _bookRepositoryMock
            .Setup(r => r.CreateBookAsync(It.IsAny<Booking>()))
            .ReturnsAsync((Booking b) =>
            {
                b.Id = 10;
                return b;
            });

        var response = await _bookManager.CreateBookAsync(dto);

        Assert.True(response.Success);
        Assert.NotNull(response.Data);
        Assert.Equal(10, response.Data.Id);
        Assert.Equal(dto.RoomId, response.Data.RoomId);
        Assert.Equal(dto.GuestId, response.Data.GuestId);
        Assert.Equal(Status.Created, response.Data.Status);
    }

    [Fact]
    public async Task CreateBookAsync_Should_Return_RoomRequired_When_Room_Does_Not_Exist()
    {
        var dto = CreateValidDto();
        var guest = CreateValidGuest();

        _bookRepositoryMock
         .Setup(r => r.GetRoomAndGuestAsync(dto.RoomId, dto.GuestId))
         .ReturnsAsync(((Domain.Room.Entities.Room?)null, guest));

        var response = await _bookManager.CreateBookAsync(dto);

        Assert.False(response.Success);
        Assert.Equal(ErrorCodes.ROOM_REQUIRED, response.ErrorCode);
        Assert.Equal("Room is required.", response.Message);
    }

    [Fact]
    public async Task CreateBookAsync_Should_Return_GuestRequired_When_Guest_Does_Not_Exist()
    {
        var dto = CreateValidDto();
        var room = CreateValidRoom();

        _bookRepositoryMock
           .Setup(r => r.GetRoomAndGuestAsync(dto.RoomId, dto.GuestId))
           .ReturnsAsync((room, (Domain.Guest.Entities.Guest?)null));

        var response = await _bookManager.CreateBookAsync(dto);

        Assert.False(response.Success);
        Assert.Equal(ErrorCodes.GUEST_REQUIRED, response.ErrorCode);
        Assert.Equal("Guest is required.", response.Message);
    }

    [Fact]
    public async Task CreateBookAsync_Should_Return_InvalidBookingDateRange_When_Start_Is_Greater_Than_End()
    {
        var dto = CreateValidDto();
        dto.Start = DateTime.UtcNow.AddHours(5);
        dto.End = DateTime.UtcNow.AddHours(2);

        var room = CreateValidRoom();
        var guest = CreateValidGuest();

        _bookRepositoryMock
            .Setup(r => r.GetRoomAndGuestAsync(dto.RoomId, dto.GuestId))
            .ReturnsAsync((room, guest));

        var response = await _bookManager.CreateBookAsync(dto);

        Assert.False(response.Success);
        Assert.Equal(ErrorCodes.INVALID_BOOKING_DATE_RANGE, response.ErrorCode);
        Assert.Equal("Start date must be earlier than end date.", response.Message);
    }

    [Fact]
    public async Task CreateBookAsync_Should_Return_BookingInPast_When_Start_Is_In_The_Past()
    {
        var dto = CreateValidDto();
        dto.Start = DateTime.UtcNow.AddHours(-2);
        dto.End = DateTime.UtcNow.AddHours(2);

        var room = CreateValidRoom();
        var guest = CreateValidGuest();

        _bookRepositoryMock
            .Setup(r => r.GetRoomAndGuestAsync(dto.RoomId, dto.GuestId))
            .ReturnsAsync((room, guest));

        var response = await _bookManager.CreateBookAsync(dto);

        Assert.False(response.Success);
        Assert.Equal(ErrorCodes.BOOKING_IN_PAST, response.ErrorCode);
        Assert.Equal("Booking cannot be created in the past.", response.Message);
    }

    [Fact]
    public async Task CreateBookAsync_Should_Return_RoomUnavailable_When_Room_Is_In_Maintenance()
    {
        var dto = CreateValidDto();
        var room = CreateValidRoom(inMaintenance: true);
        var guest = CreateValidGuest();

        _bookRepositoryMock
            .Setup(r => r.GetRoomAndGuestAsync(dto.RoomId, dto.GuestId))
            .ReturnsAsync((room, guest));

        var response = await _bookManager.CreateBookAsync(dto);

        Assert.False(response.Success);
        Assert.Equal(ErrorCodes.ROOM_UNAVAILABLE, response.ErrorCode);
        Assert.Equal("Room is unavailable.", response.Message);
    }

    [Fact]
    public async Task CreateBookAsync_Should_Return_BookingConflict_When_There_Is_Conflict()
    {
        var dto = CreateValidDto();
        var room = CreateValidRoom();
        var guest = CreateValidGuest();

        _bookRepositoryMock
            .Setup(r => r.GetRoomAndGuestAsync(dto.RoomId, dto.GuestId))
            .ReturnsAsync((room, guest));

        _bookRepositoryMock
            .Setup(r => r.HasConflictAsync(dto.RoomId, dto.Start, dto.End, null))
            .ReturnsAsync(true);

        var response = await _bookManager.CreateBookAsync(dto);

        Assert.False(response.Success);
        Assert.Equal(ErrorCodes.BOOKING_CONFLICT, response.ErrorCode);
        Assert.Equal("There is already a booking conflict for this room and period.", response.Message);
    }

    [Fact]
    public async Task CreateBookAsync_Should_Return_UnexpectedError_When_CreateBookAsync_Throws_Exception()
    {
        var dto = CreateValidDto();
        var room = CreateValidRoom();
        var guest = CreateValidGuest();

        _bookRepositoryMock
            .Setup(r => r.GetRoomAndGuestAsync(dto.RoomId, dto.GuestId))
            .ReturnsAsync((room, guest));

        _bookRepositoryMock
            .Setup(r => r.HasConflictAsync(dto.RoomId, dto.Start, dto.End, null))
            .ReturnsAsync(false);

        _bookRepositoryMock
            .Setup(r => r.CreateBookAsync(It.IsAny<Booking>()))
            .ThrowsAsync(new Exception("erro genérico"));

        var response = await _bookManager.CreateBookAsync(dto);

        Assert.False(response.Success);
        Assert.Equal(ErrorCodes.UNEXPECTED_ERROR, response.ErrorCode);
        Assert.Equal("An unexpected error occurred.", response.Message);
    }

    [Fact]
    public async Task GetById_Should_Return_Success_When_Book_Exists()
    {
        var booking = new Booking
        {
            Id = 15,
            PlacedAt = DateTime.UtcNow,
            Start = DateTime.UtcNow.AddHours(1),
            End = DateTime.UtcNow.AddHours(2),
            RoomId = 2,
            GuestId = 3
        };

        booking.ChangeState(Action.Pay); // isso só funcionaria se já estivesse Created
    }

    [Fact]
    public async Task GetById_Should_Return_NotFound_When_Book_Does_Not_Exist()
    {
        _bookRepositoryMock
            .Setup(r => r.GetBookByIdAsync(999))
            .ReturnsAsync((Booking?)null);

        var response = await _bookManager.GetById(999);

        Assert.False(response.Success);
        Assert.Equal(ErrorCodes.NOT_FOUND, response.ErrorCode);
        Assert.Equal("Book not found.", response.Message);
    }
}