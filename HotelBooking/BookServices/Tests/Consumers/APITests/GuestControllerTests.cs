using API.Controllers;
using Application;
using Application.Guest.DTO;
using Application.Guest.Port;
using Application.Guest.Request;
using Application.Guest.Responses;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;

namespace API.Tests.Controllers
{
    public class GuestControllerTests
    {
        private readonly Mock<ILogger<GuestController>> _loggerMock;
        private readonly Mock<IGuestManager> _guestManagerMock;
        private readonly GuestController _controller;

        public GuestControllerTests()
        {
            _loggerMock = new Mock<ILogger<GuestController>>();
            _guestManagerMock = new Mock<IGuestManager>();
            _controller = new GuestController(_loggerMock.Object, _guestManagerMock.Object);
        }

        private static GuestDTO CreateGuestDTO(int id = 1)
        {
            return new GuestDTO
            {
                Id = id,
                Name = "Tesla Silva",
                Email = "tesla@gmail.com",
                IdNumber = "12345678901",
                IdTypeCode = 1
            };
        }

        [Fact]
        public async Task Get_Should_Return_Ok_When_Guest_Exists()
        {
            var guestDto = CreateGuestDTO(1);

            var response = new GuestResponse
            {
                Success = true,
                Data = guestDto
            };

            _guestManagerMock
                .Setup(x => x.GetById(1))
                .ReturnsAsync(response);

            var result = await _controller.Get(1);

            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            var returnedGuest = Assert.IsType<GuestDTO>(okResult.Value);

            Assert.Equal(1, returnedGuest.Id);
            Assert.Equal("Tesla Silva", returnedGuest.Name);
            Assert.Equal("tesla@gmail.com", returnedGuest.Email);
        }

        [Fact]
        public async Task Get_Should_Return_NotFound_When_Guest_Does_Not_Exist()
        {
            var response = new GuestResponse
            {
                Success = false,
                ErrorCode = ErrorCodes.NOT_FOUND,
                Message = "Guest not found."
            };

            _guestManagerMock
                .Setup(x => x.GetById(99))
                .ReturnsAsync(response);

            var result = await _controller.Get(99);

            var notFoundResult = Assert.IsType<NotFoundObjectResult>(result.Result);
            var returnedResponse = Assert.IsType<GuestResponse>(notFoundResult.Value);

            Assert.False(returnedResponse.Success);
            Assert.Equal(ErrorCodes.NOT_FOUND, returnedResponse.ErrorCode);
            Assert.Equal("Guest not found.", returnedResponse.Message);
        }

        [Fact]
        public async Task Get_Should_Return_500_When_ErrorCode_Is_NotFound_Different()
        {
            var response = new GuestResponse
            {
                Success = false,
                ErrorCode = ErrorCodes.UNEXPECTED_ERROR,
                Message = "Unexpected error."
            };

            _guestManagerMock
                .Setup(x => x.GetById(1))
                .ReturnsAsync(response);

            var result = await _controller.Get(1);

            var objectResult = Assert.IsType<ObjectResult>(result.Result);
            Assert.Equal(StatusCodes.Status500InternalServerError, objectResult.StatusCode);

            var returnedResponse = Assert.IsType<GuestResponse>(objectResult.Value);
            Assert.False(returnedResponse.Success);
            Assert.Equal(ErrorCodes.UNEXPECTED_ERROR, returnedResponse.ErrorCode);
        }

        [Fact]
        public async Task Create_Should_Return_CreatedAtAction_When_Success()
        {
            var guestDto = CreateGuestDTO(0);

            var response = new GuestResponse
            {
                Success = true,
                Data = CreateGuestDTO(10)
            };

            _guestManagerMock
                .Setup(x => x.CreateGuest(It.IsAny<CreateGuestRequest>()))
                .ReturnsAsync(response);

            var result = await _controller.Create(guestDto);

            var createdAtActionResult = Assert.IsType<CreatedAtActionResult>(result.Result);
            Assert.Equal(nameof(GuestController.Create), createdAtActionResult.ActionName);

            var returnedResponse = Assert.IsType<GuestResponse>(createdAtActionResult.Value);
            Assert.True(returnedResponse.Success);
            Assert.Equal(10, returnedResponse.Data.Id);
        }

        [Fact]
        public async Task Create_Should_Return_BadRequest_When_ErrorCode_Is_InvalidEmail()
        {
            var guestDto = CreateGuestDTO(0);

            var response = new GuestResponse
            {
                Success = false,
                ErrorCode = ErrorCodes.INVALID_EMAIL,
                Message = "The provided email is already in use."
            };

            _guestManagerMock
                .Setup(x => x.CreateGuest(It.IsAny<CreateGuestRequest>()))
                .ReturnsAsync(response);

            var result = await _controller.Create(guestDto);

            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result.Result);
            var returnedResponse = Assert.IsType<GuestResponse>(badRequestResult.Value);

            Assert.False(returnedResponse.Success);
            Assert.Equal(ErrorCodes.INVALID_EMAIL, returnedResponse.ErrorCode);
            Assert.Equal("The provided email is already in use.", returnedResponse.Message);
        }

        [Fact]
        public async Task Create_Should_Return_BadRequest_When_ErrorCode_Is_InvalidPersonDocumentId()
        {
            var guestDto = CreateGuestDTO(0);

            var response = new GuestResponse
            {
                Success = false,
                ErrorCode = ErrorCodes.INVALID_PERSON_DOCUMENT_ID,
                Message = "The provided document id is invalid."
            };

            _guestManagerMock
                .Setup(x => x.CreateGuest(It.IsAny<CreateGuestRequest>()))
                .ReturnsAsync(response);

            var result = await _controller.Create(guestDto);

            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result.Result);
            var returnedResponse = Assert.IsType<GuestResponse>(badRequestResult.Value);

            Assert.False(returnedResponse.Success);
            Assert.Equal(ErrorCodes.INVALID_PERSON_DOCUMENT_ID, returnedResponse.ErrorCode);
            Assert.Equal("The provided document id is invalid.", returnedResponse.Message);
        }

        [Fact]
        public async Task Create_Should_Return_BadRequest_When_ErrorCode_Is_MissingRequiredInformation()
        {
            var guestDto = CreateGuestDTO(0);

            var response = new GuestResponse
            {
                Success = false,
                ErrorCode = ErrorCodes.MISSING_REQUIRED_INFORMATION,
                Message = "Missing required information."
            };

            _guestManagerMock
                .Setup(x => x.CreateGuest(It.IsAny<CreateGuestRequest>()))
                .ReturnsAsync(response);

            var result = await _controller.Create(guestDto);

            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result.Result);
            var returnedResponse = Assert.IsType<GuestResponse>(badRequestResult.Value);

            Assert.False(returnedResponse.Success);
            Assert.Equal(ErrorCodes.MISSING_REQUIRED_INFORMATION, returnedResponse.ErrorCode);
            Assert.Equal("Missing required information.", returnedResponse.Message);
        }

        [Fact]
        public async Task Create_Should_Return_500_When_ErrorCode_Is_CouldNotStoreData()
        {
            var guestDto = CreateGuestDTO(0);

            var response = new GuestResponse
            {
                Success = false,
                ErrorCode = ErrorCodes.COULD_NOT_STORE_DATA,
                Message = "Could not store data."
            };

            _guestManagerMock
                .Setup(x => x.CreateGuest(It.IsAny<CreateGuestRequest>()))
                .ReturnsAsync(response);

            var result = await _controller.Create(guestDto);

            var objectResult = Assert.IsType<ObjectResult>(result.Result);
            Assert.Equal(StatusCodes.Status500InternalServerError, objectResult.StatusCode);

            var returnedResponse = Assert.IsType<GuestResponse>(objectResult.Value);
            Assert.False(returnedResponse.Success);
            Assert.Equal(ErrorCodes.COULD_NOT_STORE_DATA, returnedResponse.ErrorCode);
            Assert.Equal("Could not store data.", returnedResponse.Message);
        }

        [Fact]
        public async Task Create_Should_Return_500_When_ErrorCode_Is_UnexpectedError()
        {
            var guestDto = CreateGuestDTO(0);

            var response = new GuestResponse
            {
                Success = false,
                ErrorCode = ErrorCodes.UNEXPECTED_ERROR,
                Message = "Unexpected error."
            };

            _guestManagerMock
                .Setup(x => x.CreateGuest(It.IsAny<CreateGuestRequest>()))
                .ReturnsAsync(response);

            var result = await _controller.Create(guestDto);

            var objectResult = Assert.IsType<ObjectResult>(result.Result);
            Assert.Equal(StatusCodes.Status500InternalServerError, objectResult.StatusCode);

            var returnedResponse = Assert.IsType<GuestResponse>(objectResult.Value);
            Assert.False(returnedResponse.Success);
            Assert.Equal(ErrorCodes.UNEXPECTED_ERROR, returnedResponse.ErrorCode);
            Assert.Equal("Unexpected error.", returnedResponse.Message);
        }

        [Fact]
        public async Task Create_Should_Return_500_When_ErrorCode_Is_Unknown()
        {
            var guestDto = CreateGuestDTO(0);

            var response = new GuestResponse
            {
                Success = false,
                ErrorCode = (ErrorCodes)999,
                Message = "Unknown error."
            };

            _guestManagerMock
                .Setup(x => x.CreateGuest(It.IsAny<CreateGuestRequest>()))
                .ReturnsAsync(response);

            var result = await _controller.Create(guestDto);

            var objectResult = Assert.IsType<ObjectResult>(result.Result);
            Assert.Equal(StatusCodes.Status500InternalServerError, objectResult.StatusCode);

            var returnedResponse = Assert.IsType<GuestResponse>(objectResult.Value);
            Assert.False(returnedResponse.Success);
            Assert.Equal("Unknown error.", returnedResponse.Message);
        }
    }
}