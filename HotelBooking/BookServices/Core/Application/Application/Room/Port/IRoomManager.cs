using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Application.Room.DTO;
using Domain.Entities;

namespace Application.Room.Port;

public interface IRoomManager
{
    Task<RoomDTO> GetRoomByIdAsync(int id);
    Task<IEnumerable<RoomDTO>> GetAllRoomsAsync();
    Task<int> AddRoomAsync(RoomDTO room);
    Task<bool> UpdateRoomAsync(RoomDTO room);
    Task<bool> DeleteRoomAsync(int id);
}
