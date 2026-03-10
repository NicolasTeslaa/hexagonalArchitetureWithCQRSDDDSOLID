using Application;
using Application.Guest.DTO;
using Application.Guest.Request;
using Application.Guest.Responses;
using Application.Guest.Services;
using Domain.Guest.Ports;
using Domain.Guest.ValueObjects;
using Moq;

namespace ApplicationTests.Guest.Services
{
    public class GuestManagerTests
    {
        private readonly Mock<IGuestRepository> _guestRepositoryMock;
        private readonly GuestManager _guestManager;

        public GuestManagerTests()
        {
            _guestRepositoryMock = new Mock<IGuestRepository>();
            _guestManager = new GuestManager(_guestRepositoryMock.Object);
        }

        private CreateGuestRequest CreateValidRequest()
        {
            return new CreateGuestRequest
            {
                Data = new GuestDTO
                {
                    Id = 0,
                    Name = "Tesla Silva",
                    Email = "tesla@gmail.com",
                    IdNumber = "12345678901",
                    IdTypeCode = 1
                }
            };
        }
        [Fact]
        public async Task GetById_Should_Return_Success_When_Guest_Exists()
        {
            var guest = new Domain.Guest.Entities.Guest
            {
                Id = 15,
                Name = "Tesla Silva",
                Email = "tesla@gmail.com",
                DocumentId = new Domain.Guest.ValueObjects.PersonId
                {
                    IdNumber = "12345678901",
                    Document = (DocumentType)1
                }
            };

            _guestRepositoryMock
                .Setup(r => r.FindById(15))
                .ReturnsAsync(guest);

            GuestResponse response = await _guestManager.GetById(15);

            Assert.True(response.Success);
            Assert.NotNull(response.Data);
            Assert.Equal(15, response.Data.Id);
            Assert.Equal("Tesla Silva", response.Data.Name);
            Assert.Equal("tesla@gmail.com", response.Data.Email);
            Assert.Equal("12345678901", response.Data.IdNumber);
            Assert.Equal(1, response.Data.IdTypeCode);

            _guestRepositoryMock.Verify(r => r.FindById(15), Times.Once);
        }

        [Fact]
        public async Task GetById_Should_Return_NotFound_When_Guest_Does_Not_Exist()
        {
            _guestRepositoryMock
                .Setup(r => r.FindById(15))
                .ReturnsAsync((Domain.Guest.Entities.Guest?)null);

            GuestResponse response = await _guestManager.GetById(15);

            Assert.False(response.Success);
            Assert.Equal(ErrorCodes.NOT_FOUND, response.ErrorCode);
            Assert.Equal("Guest not found.", response.Message);
            Assert.Null(response.Data);

            _guestRepositoryMock.Verify(r => r.FindById(15), Times.Once);
        }

        [Fact]
        public async Task GetById_Should_Return_UnexpectedError_When_Repository_Throws_Exception()
        {
            _guestRepositoryMock
                .Setup(r => r.FindById(15))
                .ThrowsAsync(new Exception("database failure"));

            GuestResponse response = await _guestManager.GetById(15);

            Assert.False(response.Success);
            Assert.Equal(ErrorCodes.UNEXPECTED_ERROR, response.ErrorCode);
            Assert.Equal("Not able to retrieve the guest data. Please try again later.", response.Message);

            _guestRepositoryMock.Verify(r => r.FindById(15), Times.Once);
        }
        [Fact]
        public async Task CreateGuest_Should_Return_Success_When_Request_Is_Valid()
        {
            var request = CreateValidRequest();

            _guestRepositoryMock
                .Setup(r => r.FindByEmail(request.Data.Email))
                .ReturnsAsync((Domain.Guest.Entities.Guest?)null);

            _guestRepositoryMock
                .Setup(r => r.AddGuestAsync(It.IsAny<Domain.Guest.Entities.Guest>()))
                .ReturnsAsync(15);

            GuestResponse response = await _guestManager.CreateGuest(request);

            Assert.True(response.Success);
            Assert.NotNull(response.Data);
            Assert.Equal(15, response.Data.Id);
            Assert.Equal(request.Data.Email, response.Data.Email);
            Assert.Equal(request.Data.Name, response.Data.Name);

            _guestRepositoryMock.Verify(r => r.FindByEmail(request.Data.Email), Times.Once);
            _guestRepositoryMock.Verify(r => r.AddGuestAsync(It.IsAny<Domain.Guest.Entities.Guest>()), Times.Once);
        }

        [Fact]
        public async Task CreateGuest_Should_Return_InvalidEmail_When_Email_Already_Exists()
        {
            var request = CreateValidRequest();

            _guestRepositoryMock
                .Setup(r => r.FindByEmail(request.Data.Email))
                .ReturnsAsync(new Domain.Guest.Entities.Guest
                {
                    Id = 99,
                    Name = "Existing Guest",
                    Email = request.Data.Email
                });

            GuestResponse response = await _guestManager.CreateGuest(request);

            Assert.False(response.Success);
            Assert.Equal(ErrorCodes.INVALID_EMAIL, response.ErrorCode);
            Assert.Equal("The provided email is already in use.", response.Message);

            _guestRepositoryMock.Verify(r => r.FindByEmail(request.Data.Email), Times.Once);
            _guestRepositoryMock.Verify(r => r.AddGuestAsync(It.IsAny<Domain.Guest.Entities.Guest>()), Times.Never);
        }

        [Fact]
        public async Task CreateGuest_Should_Return_InvalidPersonDocumentId_When_IdNumber_Is_Null()
        {
            var request = CreateValidRequest();
            request.Data.IdNumber = null!;

            GuestResponse response = await _guestManager.CreateGuest(request);

            Assert.False(response.Success);
            Assert.Equal(ErrorCodes.INVALID_PERSON_DOCUMENT_ID, response.ErrorCode);
            Assert.Equal("The provided document id is not valid.", response.Message);

            _guestRepositoryMock.Verify(r => r.FindByEmail(It.IsAny<string>()), Times.Never);
            _guestRepositoryMock.Verify(r => r.AddGuestAsync(It.IsAny<Domain.Guest.Entities.Guest>()), Times.Never);
        }

        [Fact]
        public async Task CreateGuest_Should_Return_InvalidPersonDocumentId_When_IdNumber_Is_TooShort()
        {
            var request = CreateValidRequest();
            request.Data.IdNumber = "123";

            GuestResponse response = await _guestManager.CreateGuest(request);

            Assert.False(response.Success);
            Assert.Equal(ErrorCodes.INVALID_PERSON_DOCUMENT_ID, response.ErrorCode);
            Assert.Equal("The provided document id is not valid.", response.Message);
        }

        [Fact]
        public async Task CreateGuest_Should_Return_InvalidPersonDocumentId_When_IdTypeCode_Is_Zero()
        {
            var request = CreateValidRequest();
            request.Data.IdTypeCode = 0;

            GuestResponse response = await _guestManager.CreateGuest(request);

            Assert.False(response.Success);
            Assert.Equal(ErrorCodes.INVALID_PERSON_DOCUMENT_ID, response.ErrorCode);
            Assert.Equal("The provided document id is not valid.", response.Message);
        }

        [Fact]
        public async Task CreateGuest_Should_Return_MissingRequiredInformation_When_Name_Is_Null()
        {
            var request = CreateValidRequest();
            request.Data.Name = null!;

            GuestResponse response = await _guestManager.CreateGuest(request);

            Assert.False(response.Success);
            Assert.Equal(ErrorCodes.MISSING_REQUIRED_INFORMATION, response.ErrorCode);
            Assert.Equal("Missing required information.", response.Message);
        }

        [Fact]
        public async Task CreateGuest_Should_Return_MissingRequiredInformation_When_Name_Is_TooShort()
        {
            var request = CreateValidRequest();
            request.Data.Name = "Abc";

            GuestResponse response = await _guestManager.CreateGuest(request);

            Assert.False(response.Success);
            Assert.Equal(ErrorCodes.MISSING_REQUIRED_INFORMATION, response.ErrorCode);
            Assert.Equal("Missing required information.", response.Message);
        }

        [Fact]
        public async Task CreateGuest_Should_Return_InvalidEmail_When_Email_Is_Invalid()
        {
            var request = CreateValidRequest();
            request.Data.Email = "emailinvalido";

            GuestResponse response = await _guestManager.CreateGuest(request);

            Assert.False(response.Success);
            Assert.Equal(ErrorCodes.INVALID_EMAIL, response.ErrorCode);
            Assert.Equal("The provided email is not valid.", response.Message);

            _guestRepositoryMock.Verify(r => r.FindByEmail(It.IsAny<string>()), Times.Never);
        }

        [Fact]
        public async Task CreateGuest_Should_Return_UnexpectedError_When_Repository_Throws_Exception()
        {
            var request = CreateValidRequest();

            _guestRepositoryMock
                .Setup(r => r.FindByEmail(request.Data.Email))
                .ThrowsAsync(new Exception("database failure"));

            GuestResponse response = await _guestManager.CreateGuest(request);

            Assert.False(response.Success);
            Assert.Equal(ErrorCodes.UNEXPECTED_ERROR, response.ErrorCode);
            Assert.Equal("Not able to store the guest data. Please try again later.", response.Message);
        }
    }
}