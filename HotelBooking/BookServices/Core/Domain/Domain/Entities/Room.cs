using Domain.Enums;
using Domain.Exceptions;
using Domain.Ports;
using Domain.ValueObjects;

namespace Domain.Entities;

public class Room
{
    public int Id { get; set; }
    public string Name { get; set; }
    public int Level { get; set; }
    public bool InMaintenance { get; set; }
    public Price Price { get; set; }
    public bool IsAvaliable
    {
        get
        {
            if (this.InMaintenance || this.HasGuest)
            {
                return false;
            }

            return true;
        }
    }
    public bool HasGuest { get { return true; } }

    private void Validate()
    {
        if (string.IsNullOrWhiteSpace(Name) ||
            Name.Trim().Length < 3 ||
            Name.Trim().Length > 100 ||
            Level <= 0 || Price == null ||
            Price.Value <= 0 ||
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