using Application.Guest.Responses;
using Application.Room.DTO;
using Application.Room.Port;
using Application.Room.Request;
using Application.Room.Responses;
using Domain.Exceptions;
using Domain.Room.Ports;

namespace Application.Room.Services;

public class RoomManager : IRoomManager
{
    private readonly IRoomRepository _roomRepository;
    public RoomManager(IRoomRepository roomRepository) => _roomRepository = roomRepository;

    public async Task<RoomResponse> AddRoomAsync(CreateRoomRequest room)
    {
        try
        {
            var roomEntity = RoomDTO.MapToEntity(room.Data);

            await roomEntity.Save(_roomRepository);

            return new RoomResponse
            {
                Data = room.Data,
                Success = true
            };
        }
        catch (MissingRequiredInformationException)
        {
            return new RoomResponse
            {
                Success = false,
                ErrorCode = ErrorCodes.MISSING_REQUIRED_INFORMATION,
                Message = "Missing required information."
            };
        }
        catch (Exception)
        {
            return new RoomResponse
            {
                Success = false,
                ErrorCode = ErrorCodes.UNEXPECTED_ERROR,
                Message = "Intern Error."
            };
        }
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

    public async Task<RoomResponse> GetRoomByIdAsync(int id)
    {
        var roomEntity = await _roomRepository.GetRoomByIdAsync(id);

        if(roomEntity is null)
        {
            return new RoomResponse
            {
                Success = false,
                ErrorCode = ErrorCodes.NOT_FOUND,
                Message = "Room not found"
            };
        }

        RoomDTO roomDTO = RoomDTO.MapFromEntity(roomEntity);

        RoomResponse response = new RoomResponse
        {
            Data = roomDTO,
            Success = true
        };

        return response;
    }

    public async Task<bool> UpdateRoomAsync(RoomDTO room)
    {
        var roomEntity = RoomDTO.MapToEntity(room);

        await roomEntity.Save(_roomRepository);

        return roomEntity.Id > 0 ? true : false;
    }
}
