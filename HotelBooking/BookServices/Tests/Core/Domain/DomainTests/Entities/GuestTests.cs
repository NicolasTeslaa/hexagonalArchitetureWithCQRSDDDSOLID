using Domain.Entities;
using Domain.Exceptions;
using Domain.Ports;
using Domain.ValueObjects;
using Moq;
using Xunit;

namespace DomainTests.Entities
{
    public class GuestTests
    {
        private readonly Mock<IGuestRepository> _guestRepositoryMock;

        public GuestTests()
        {
            _guestRepositoryMock = new Mock<IGuestRepository>();
        }

        private Guest CreateValidGuest()
        {
            return new Guest
            {
                Id = 0,
                Name = "Tesla Silva",
                Email = "tesla@gmail.com",
                DocumentId = new PersonId
                {
                    IdNumber = "12345678901",
                    Document = (DocumentType)1
                }
            };
        }

        [Fact]
        public async Task Save_Should_AddGuest_When_Id_Is_Zero_And_Data_Is_Valid()
        {
            var guest = CreateValidGuest();

            _guestRepositoryMock
                .Setup(r => r.FindByEmail(guest.Email))
                .ReturnsAsync(false);

            _guestRepositoryMock
                .Setup(r => r.AddGuestAsync(It.IsAny<Guest>()))
                .ReturnsAsync(10);

            await guest.Save(_guestRepositoryMock.Object);

            Assert.Equal(10, guest.Id);

            _guestRepositoryMock.Verify(r => r.FindByEmail(guest.Email), Times.Once);
            _guestRepositoryMock.Verify(r => r.AddGuestAsync(It.IsAny<Guest>()), Times.Once);
            _guestRepositoryMock.Verify(r => r.UpdateGuestAsync(It.IsAny<Guest>()), Times.Never);
        }

        [Fact]
        public async Task Save_Should_UpdateGuest_When_Id_Is_Not_Zero_And_Data_Is_Valid()
        {
            var guest = CreateValidGuest();
            guest.Id = 5;

            _guestRepositoryMock
                .Setup(r => r.FindByEmail(guest.Email))
                .ReturnsAsync(false);

            await guest.Save(_guestRepositoryMock.Object);

            Assert.Equal(5, guest.Id);

            _guestRepositoryMock.Verify(r => r.FindByEmail(guest.Email), Times.Once);
            _guestRepositoryMock.Verify(r => r.UpdateGuestAsync(It.IsAny<Guest>()), Times.Once);
            _guestRepositoryMock.Verify(r => r.AddGuestAsync(It.IsAny<Guest>()), Times.Never);
        }

        [Fact]
        public async Task Save_Should_Throw_InvalidPersonDocumentIdException_When_DocumentId_Is_Null()
        {
            var guest = CreateValidGuest();
            guest.DocumentId = null!;

            await Assert.ThrowsAsync<InvalidPersonDocumentIdException>(() =>
                guest.Save(_guestRepositoryMock.Object));

            _guestRepositoryMock.Verify(r => r.FindByEmail(It.IsAny<string>()), Times.Never);
            _guestRepositoryMock.Verify(r => r.AddGuestAsync(It.IsAny<Guest>()), Times.Never);
            _guestRepositoryMock.Verify(r => r.UpdateGuestAsync(It.IsAny<Guest>()), Times.Never);
        }

        [Fact]
        public async Task Save_Should_Throw_InvalidPersonDocumentIdException_When_IdNumber_Is_Invalid()
        {
            var guest = CreateValidGuest();
            guest.DocumentId = new PersonId
            {
                IdNumber = "123",
                Document = (DocumentType)1
            };

            await Assert.ThrowsAsync<InvalidPersonDocumentIdException>(() =>
                guest.Save(_guestRepositoryMock.Object));

            _guestRepositoryMock.Verify(r => r.FindByEmail(It.IsAny<string>()), Times.Never);
        }

        [Fact]
        public async Task Save_Should_Throw_InvalidPersonDocumentIdException_When_Document_Is_Zero()
        {
            var guest = CreateValidGuest();
            guest.DocumentId = new PersonId
            {
                IdNumber = "12345678901",
                Document = 0
            };

            await Assert.ThrowsAsync<InvalidPersonDocumentIdException>(() =>
                guest.Save(_guestRepositoryMock.Object));

            _guestRepositoryMock.Verify(r => r.FindByEmail(It.IsAny<string>()), Times.Never);
        }

        [Fact]
        public async Task Save_Should_Throw_MissingRequiredInformationException_When_Name_Is_Null()
        {
            var guest = CreateValidGuest();
            guest.Name = null!;

            await Assert.ThrowsAsync<MissingRequiredInformationException>(() =>
                guest.Save(_guestRepositoryMock.Object));

            _guestRepositoryMock.Verify(r => r.FindByEmail(It.IsAny<string>()), Times.Never);
        }

        [Fact]
        public async Task Save_Should_Throw_MissingRequiredInformationException_When_Name_Is_TooShort()
        {
            var guest = CreateValidGuest();
            guest.Name = "Abc";

            await Assert.ThrowsAsync<MissingRequiredInformationException>(() =>
                guest.Save(_guestRepositoryMock.Object));

            _guestRepositoryMock.Verify(r => r.FindByEmail(It.IsAny<string>()), Times.Never);
        }

        [Fact]
        public async Task Save_Should_Throw_InvalidEmailException_When_Email_Is_Invalid()
        {
            var guest = CreateValidGuest();
            guest.Email = "emailinvalido";

            await Assert.ThrowsAsync<InvalidEmailException>(() =>
                guest.Save(_guestRepositoryMock.Object));

            _guestRepositoryMock.Verify(r => r.FindByEmail(It.IsAny<string>()), Times.Never);
        }

        [Fact]
        public async Task Save_Should_Throw_EmailAlreadyUseException_When_Email_Already_Exists()
        {
            var guest = CreateValidGuest();

            _guestRepositoryMock
                .Setup(r => r.FindByEmail(guest.Email))
                .ReturnsAsync(true);

            await Assert.ThrowsAsync<EmailAlreadyUseException>(() =>
                guest.Save(_guestRepositoryMock.Object));

            _guestRepositoryMock.Verify(r => r.FindByEmail(guest.Email), Times.Once);
            _guestRepositoryMock.Verify(r => r.AddGuestAsync(It.IsAny<Guest>()), Times.Never);
            _guestRepositoryMock.Verify(r => r.UpdateGuestAsync(It.IsAny<Guest>()), Times.Never);
        }
    }
}