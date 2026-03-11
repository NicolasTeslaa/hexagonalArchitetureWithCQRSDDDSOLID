using Domain.Book.Enums;

namespace Application.Book.DTO;

public class BookDTO
{
    public int Id { get; set; }
    public DateTime PlacedAt { get; set; }
    public DateTime Start { get; set; }
    public DateTime End { get; set; }
    public Status Status { get; set; }
    public int RoomId { get; set; }
    public int GuestId { get; set; }

    public static Domain.Book.Entities.Booking MapToEntity(BookDTO dto)
    {
        return new Domain.Book.Entities.Booking
        {
            Id = dto.Id,
            PlacedAt = dto.PlacedAt,
            Start = dto.Start,
            End = dto.End,
            RoomId = dto.RoomId,
            GuestId = dto.GuestId,
        };
    }

    public static BookDTO MapFromEntity(Domain.Book.Entities.Booking dto)
    {
        return new BookDTO
        {
            Id = dto.Id,
            PlacedAt = dto.PlacedAt,
            Start = dto.Start,
            End = dto.End,
            Status = dto.CurrentStatus,
            RoomId = dto.RoomId,
            GuestId = dto.GuestId,
        };
    }
}
