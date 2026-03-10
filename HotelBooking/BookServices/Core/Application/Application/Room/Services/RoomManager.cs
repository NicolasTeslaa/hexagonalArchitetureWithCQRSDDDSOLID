using Application.Room.DTO;
using Application.Room.Port;
using Domain.Ports;

namespace Application.Room.Services;

public class RoomManager : IRoomManager
{
    private readonly IRoomRepository _roomRepository;
    public RoomManager(IRoomRepository roomRepository) => _roomRepository = roomRepository;

    public async Task<int> AddRoomAsync(RoomDTO room)
    {
        var roomEntity = RoomDTO.MapToEntity(room);

        await roomEntity.Save(_roomRepository);

        return roomEntity.Id;
    }

    public async Task<bool> DeleteRoomAsync(int id)
    {
        return await _roomRepository.DeleteRoomAsync(id);
    }

    public async Task<IEnumerable<RoomDTO>> GetAllRoomsAsync()
    {
        var list = await _roomRepository.GetAllRoomsAsync();

        return list.Select(r => RoomDTO.MapFromEntity(r));
    }

    public async Task<RoomDTO> GetRoomByIdAsync(int id)
    {
        var roomEntity = _roomRepository.GetRoomByIdAsync(id);

        RoomDTO roomDTO = RoomDTO.MapFromEntity(roomEntity.Result);

        return roomDTO;
    }

    public async Task<bool> UpdateRoomAsync(RoomDTO room)
    {
        var roomEntity = RoomDTO.MapToEntity(room);

        await roomEntity.Save(_roomRepository);

        return roomEntity.Id > 0 ? true : false;
    }
}
