using Domain.Guest.Entities;
using Domain.Guest.Ports;
using Microsoft.EntityFrameworkCore;

namespace Data.MySql.Repositories;

public class GuestRepository : IGuestRepository
{
    private readonly HotelDbContext _context;
    public GuestRepository(HotelDbContext context) => _context = context;

    public async Task<int> AddGuestAsync(Guest guest)
    {
        await _context.Guest.AddAsync(guest);
        await _context.SaveChangesAsync();
        return guest.Id;
    }

    public async Task<bool> DeleteGuestAsync(int id)
    {
        var guest = await _context.Guest.FirstOrDefaultAsync(g => g.Id == id);

        if (guest is null) return false;

        _context.Guest.Remove(guest);
        await _context.SaveChangesAsync();

        return true;
    }

    public async Task<Guest?> FindByEmail(string email) => await _context.Guest.FirstOrDefaultAsync(g => g.Email == email) ?? null;

    public async Task<Guest?> FindById(int id) => await _context.Guest.FirstOrDefaultAsync(g => g.Id == id) ?? null;

    public async Task<IEnumerable<Guest>> GetAllGuestsAsync() => await _context.Guest.ToListAsync();

    public async Task<Guest> GetGuestByIdAsync(int id) => await _context.Guest.FirstOrDefaultAsync(g => g.Id == id) ?? new Guest();

    public async Task<bool> UpdateGuestAsync(Guest guest)
    {
        _context.Guest.Update(guest);
        await _context.SaveChangesAsync();
        return true;
    }
}
