using Domain.Entities;

namespace Domain.Ports;

public interface IGuestRepository
{
    Task<Guest> GetGuestByIdAsync(int id);
    Task<IEnumerable<Guest>> GetAllGuestsAsync();
    Task<int> AddGuestAsync(Guest guest);
    Task<bool> UpdateGuestAsync(Guest guest);
    Task<bool> DeleteGuestAsync(int id);
    Task<Guest?> FindByEmail(string email);
    Task<Guest?> FindById(int id);
}
