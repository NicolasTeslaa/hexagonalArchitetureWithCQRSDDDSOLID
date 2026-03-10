namespace Domain.Room.Ports;

public interface IRoomRepository
{
    Task<Entities.Room?> GetRoomByIdAsync(int id);
    Task<IEnumerable<Entities.Room>> GetAllRoomsAsync();
    Task<int> AddRoomAsync(Entities.Room room);
    Task<bool> UpdateRoomAsync(Entities.Room room);
    Task<bool> DeleteRoomAsync(int id);
}
