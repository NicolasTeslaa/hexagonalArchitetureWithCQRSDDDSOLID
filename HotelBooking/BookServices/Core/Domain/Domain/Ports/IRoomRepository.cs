namespace Domain.Ports;

public interface IRoomRepository
{
    Task<Domain.Entities.Room> GetRoomByIdAsync(int id);
    Task<IEnumerable<Domain.Entities.Room>> GetAllRoomsAsync();
    Task<int> AddRoomAsync(Domain.Entities.Room room);
    Task<bool> UpdateRoomAsync(Domain.Entities.Room room);
    Task<bool> DeleteRoomAsync(int id);
}
