using Domain.Book.Enums;
using Domain.Book.Exceptions;
using Domain.Book.Ports;
using Action = Domain.Book.Enums.Action;

namespace Domain.Book.Entities;

public class Booking
{
    public int Id { get; set; }
    public DateTime PlacedAt { get; set; }
    public DateTime Start { get; set; }
    public DateTime End { get; set; }
    private Status Status { get; set; }
    public int RoomId { get; set; }
    public int GuestId { get; set; }
    public Status CurrentStatus => this.Status;

    private async Task ValidateState(IBookRepository repository)
    {
        var (room, guest) = await repository.GetRoomAndGuestAsync(RoomId, GuestId);

        if (room is null || room.Id is 0)
            throw new RoomRequiredException();

        if (guest is null || guest.Id is 0)
            throw new GuestRequiredException();

        if (Start >= End)
            throw new InvalidBookingDateRangeException();

        if (Start < DateTime.UtcNow)
            throw new BookingInPastException();

        if (!room.IsAvaliable)
            throw new RoomUnavailableException();

        var hasConflict = await repository.HasConflictAsync(room.Id, Start, End, Id is 0 ? null : Id);

        if (hasConflict)
            throw new BookingConflictException();
    }

    public async Task Save(IBookRepository repository)
    {
        await ValidateState(repository);

        if (PlacedAt == default)
            PlacedAt = DateTime.UtcNow;

        if (Id == 0)
        {
            Status = Status.Created;

            var book = await repository.CreateBookAsync(this);

            Id = book.Id;

            return;
        }

        await repository.UpdateBookAsync(this);
    }

    public void ChangeState(Action action)
    {
        var newStatus = (Status, action) switch
        {
            (Status.Created, Action.Pay) => Status.Paid,
            (Status.Created, Action.Cancel) => Status.Canceled,
            (Status.Paid, Action.Finish) => Status.Finished,
            (Status.Paid, Action.Refound) => Status.Refunded,
            (Status.Canceled, Action.ReOpen) => Status.Created,
            _ => throw new InvalidBookingStateTransitionException(Status, action)
        };

        Status = newStatus;
    }
}