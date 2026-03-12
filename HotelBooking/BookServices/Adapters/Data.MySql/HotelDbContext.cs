using Data.MySql.Configuration;
using Domain.Book.Entities;
using Domain.Guest.Entities;
using Domain.Room.Entities;
using Microsoft.EntityFrameworkCore;

namespace Data.MySql;

public class HotelDbContext : DbContext
{
    public HotelDbContext(DbContextOptions<HotelDbContext> options) : base(options){}

    public virtual DbSet<Booking> Book{ get; set; }
    public virtual DbSet<Room> Room { get; set; }
    public virtual DbSet<Guest> Guest { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfiguration(new GuestConfiguration());
        modelBuilder.ApplyConfiguration(new RoomConfiguration());
        modelBuilder.ApplyConfiguration(new BookingConfiguration());
    }
}
