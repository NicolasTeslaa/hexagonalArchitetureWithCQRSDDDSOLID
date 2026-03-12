using Domain.Book.Entities;
using Domain.Room.ValueObjects;
using System.Text.Json.Serialization;

namespace Application.Room.DTO;

public class RoomDTO
{
    public int Id { get; set; }
    public string Name { get; set; }
    public int Level { get; set; }
    public bool InMaintenance { get; set; }
    public Price Price { get; set; }
    public bool IsAvaliable { get; set; }
    public bool HasGuest { get; set; }
    public static Domain.Room.Entities.Room MapToEntity(RoomDTO roomDTO)
    {
        return new Domain.Room.Entities.Room
        {
            Id = roomDTO.Id,
            Name = roomDTO.Name,
            Level = roomDTO.Level,
            InMaintenance = roomDTO.InMaintenance,
            Price = roomDTO.Price,
        };
    }

    public static RoomDTO MapFromEntity(Domain.Room.Entities.Room room)
    {
        return new RoomDTO
        {
            Id = room.Id,
            Name = room.Name,
            Level = room.Level,
            InMaintenance = room.InMaintenance,
            Price = room.Price,
            IsAvaliable = room.IsAvaliable,
            HasGuest = room.HasGuest
        };
    }
}