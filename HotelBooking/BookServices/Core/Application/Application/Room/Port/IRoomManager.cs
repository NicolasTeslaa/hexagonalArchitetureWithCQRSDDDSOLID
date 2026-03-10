using Application.Room.DTO;
using Application.Room.Request;
using Application.Room.Responses;

namespace Application.Room.Port;

public interface IRoomManager
{
    Task<RoomResponse> GetRoomByIdAsync(int id);
    Task<IEnumerable<RoomDTO>> GetAllRoomsAsync();
    Task<RoomResponse> AddRoomAsync(CreateRoomRequest room);
    Task<bool> UpdateRoomAsync(RoomDTO room);
    Task<bool> DeleteRoomAsync(int id);
}
