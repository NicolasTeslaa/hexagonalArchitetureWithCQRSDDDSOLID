using Application.Guest.Request;
using Application.Guest.Responses;

namespace Application.Guest.Port;

public interface IGuestManager
{
    Task<GuestResponse> CreateGuest(CreateGuestRequest guest);
    Task<GuestResponse> GetById(int id);
}
