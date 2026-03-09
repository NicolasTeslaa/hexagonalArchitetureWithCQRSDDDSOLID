using Domain.Entities;
using Domain.Ports;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Data.MySql.Repositories
{
    public class GuestRepository : IGuestRepository
    {
        private readonly HotelDbContext _context;

        public GuestRepository(HotelDbContext context) => _context = context;

        public async Task<int> AddGuestAsync(Guest guest)
        {
            await _context.AddAsync(guest);
            await _context.SaveChangesAsync();
            return guest.Id;
        }

        public Task<bool> DeleteGuestAsync(int id)
        {
            throw new NotImplementedException();
        }

        public Task<IEnumerable<Guest>> GetAllGuestsAsync()
        {
            throw new NotImplementedException();
        }

        public Task<Guest> GetGuestByIdAsync(int id)
        {
            throw new NotImplementedException();
        }

        public Task<bool> UpdateGuestAsync(Guest guest)
        {
            throw new NotImplementedException();
        }
    }
}
