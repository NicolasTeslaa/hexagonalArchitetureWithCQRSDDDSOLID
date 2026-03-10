using Application.Room.DTO;
using Application.Room.Port;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Room.Services;

public class RoomManager : IRoomManager
{
    public Task<int> AddRoomAsync(RoomDTO room)
    {
        throw new NotImplementedException();
    }

    public Task<bool> DeleteRoomAsync(int id)
    {
        throw new NotImplementedException();
    }

    public Task<IEnumerable<Domain.Entities.Room>> GetAllRoomsAsync()
    {
        throw new NotImplementedException();
    }

    public Task<Domain.Entities.Room> GetRoomByIdAsync(int id)
    {
        throw new NotImplementedException();
    }

    public Task<bool> UpdateRoomAsync(RoomDTO room)
    {
        throw new NotImplementedException();
    }
}
