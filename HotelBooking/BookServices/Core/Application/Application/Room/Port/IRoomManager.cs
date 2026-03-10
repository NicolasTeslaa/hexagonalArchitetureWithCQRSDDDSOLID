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
    Task<Domain.Entities.Room> GetRoomByIdAsync(int id);
    Task<IEnumerable<Domain.Entities.Room>> GetAllRoomsAsync();
    Task<int> AddRoomAsync(RoomDTO room);
    Task<bool> UpdateRoomAsync(RoomDTO room);
    Task<bool> DeleteRoomAsync(int id);
}
