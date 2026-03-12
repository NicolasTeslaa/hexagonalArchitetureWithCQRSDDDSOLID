using Domain.Book.Entities;
using Domain.Exceptions;
using Domain.Guest.Exceptions;
using Domain.Guest.Ports;
using Domain.Guest.ValueObjects;
using System.Collections.ObjectModel;
using System.Text.Json.Serialization;

namespace Domain.Guest.Entities;

public class Guest
{
    public int Id { get; set; }
    public string Name { get; set; }
    public string Email { get; set; }
    [JsonIgnore]
    public Collection<Booking> Bookings { get; set; }
    public PersonId DocumentId { get; set; }

    private async Task ValidateState(IGuestRepository repository)
    {
        if (DocumentId is null || string.IsNullOrWhiteSpace(DocumentId.IdNumber) || DocumentId.IdNumber.Length <= 3 || DocumentId.Document is 0)
            throw new InvalidPersonDocumentIdException();

        if (string.IsNullOrWhiteSpace(Name) || Name.Length <= 3)
            throw new MissingRequiredInformationException();

        if (!Utils.Utils.ValidateEmail(Email))
            throw new InvalidEmailException();

        if (await repository.FindByEmail(Email) is not null)
            throw new EmailAlreadyUseException();
    }

    public async Task Save(IGuestRepository repository)
    {
        await ValidateState(repository);

        if (this.Id is 0)
            this.Id = await repository.AddGuestAsync(this);
        else
            await repository.UpdateGuestAsync(this);
    }
}