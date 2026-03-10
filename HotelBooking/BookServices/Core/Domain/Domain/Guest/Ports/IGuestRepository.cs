namespace Domain.Guest.Ports;

public interface IGuestRepository
{
    Task<Entities.Guest> GetGuestByIdAsync(int id);
    Task<IEnumerable<Entities.Guest>> GetAllGuestsAsync();
    Task<int> AddGuestAsync(Entities.Guest guest);
    Task<bool> UpdateGuestAsync(Entities.Guest guest);
    Task<bool> DeleteGuestAsync(int id);
    Task<Entities.Guest?> FindByEmail(string email);
    Task<Entities.Guest?> FindById(int id);
}
