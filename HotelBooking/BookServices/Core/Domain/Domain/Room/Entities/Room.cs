using Domain.Book.Entities;
using Domain.Exceptions;
using Domain.Room.Enums;
using Domain.Room.Ports;
using Domain.Room.ValueObjects;

namespace Domain.Room.Entities;

public class Room
{
    public int Id { get; set; }
    public string Name { get; set; }
    public int Level { get; set; }
    public bool InMaintenance { get; set; }
    public Price Price { get; set; }
    public ICollection<Booking> Bookings { get; set; }
    public bool IsAvaliable
    {
        get
        {
            if (InMaintenance || HasGuest)
                return false;

            return true;
        }
    }

    public bool HasGuest
    {
        get
        {
            if (Bookings is null || !Bookings.Any())
                return false;

            var now = DateTime.UtcNow;

            var occupiedStatuses = new[]
            {
            Domain.Book.Enums.Status.Created,
            Domain.Book.Enums.Status.Paid
        };

            return Bookings.Any(b =>
                b.RoomId == Id &&
                occupiedStatuses.Contains(b.CurrentStatus) &&
                b.Start <= now &&
                b.End >= now);
        }
    }

    private void Validate()
    {
        if (string.IsNullOrWhiteSpace(Name) ||
            Name.Trim().Length < 3 ||
            Name.Trim().Length > 100 ||
            Level <= 0 || Price == null ||
            Price.Value <= 10 ||
            !Enum.IsDefined(typeof(AcceptedCurrencies), Price.Currency))
            throw new MissingRequiredInformationException();
    }

    public async Task Save(IRoomRepository repository)
    {
        Validate();

        if (this.Id == 0)
        {
            this.Id = await repository.AddRoomAsync(this);
            return;
        }

        await repository.UpdateRoomAsync(this);
    }
}