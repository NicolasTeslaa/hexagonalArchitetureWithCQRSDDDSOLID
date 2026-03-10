using Domain.Entities;
using Domain.Ports;
using Microsoft.EntityFrameworkCore;

namespace Data.MySql.Repositories;

public class RoomRepository : IRoomRepository
{
    private readonly HotelDbContext _context;
    public RoomRepository(HotelDbContext context) => _context = context;

    public async Task<int> AddRoomAsync(Room room)
    {
        _context.Room.Add(room);
        await _context.SaveChangesAsync();
        return room.Id;
    }

    public async Task<bool> DeleteRoomAsync(int id)
    {
        var room = await _context.Room.FindAsync(id);
        
        if (room == null)
            return false;

        _context.Room.Remove(room);

        await _context.SaveChangesAsync();

        return true;
    }

    public async Task<IEnumerable<Room>> GetAllRoomsAsync()
    {
        return await _context.Room.ToListAsync();
    }

    public async Task<Room> GetRoomByIdAsync(int id)
    {
        var room = await _context.Room.FindAsync(id);
        return room ?? new Room();
    }

    public async Task<bool> UpdateRoomAsync(Room room)
    {
        var existingRoom = await _context.Room.FindAsync(room.Id);
        
        if (existingRoom == null)
            return false;

        existingRoom.Name = room.Name;
        existingRoom.Level = room.Level;
        existingRoom.InMaintenance = room.InMaintenance;
        existingRoom.Price = room.Price;

        _context.Room.Update(existingRoom);
        await _context.SaveChangesAsync();
        return true;
    }
}
