using Application.Guest.DTO;
using Application.Guest.Port;
using Application.Guest.Request;
using Application.Guest.Responses;
using Domain.Ports;

namespace Application.Guest.Services;

public class GuestManager : IGuestManager
{
    private IGuestRepository _guestRepository;
    public GuestManager(IGuestRepository guestRepository) => _guestRepository = guestRepository;

    public async Task<GuestResponse> CreateGuest(CreateGuestRequest request)
    {
        try
        {
            Domain.Entities.Guest guest = GuestDTO.MapToEntity(request.Data);

            guest.Id = await _guestRepository.AddGuestAsync(guest);

            return new GuestResponse
            {
                Data = request.Data,
                Success = true
            };
        }
        catch (Exception)
        {
            return new GuestResponse
            {
                Success = false,
                ErrorCode = ErrorCodes.COULD_NOT_STORE_DATA,
                Message = "Not able to store the guest data. Please try again later."
            };
        }
    }
}