using Domain.Room.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Data.MySql.Configuration;

public class RoomConfiguration : IEntityTypeConfiguration<Room>
{
    public void Configure(EntityTypeBuilder<Room> builder)
    {
        builder.HasKey(x=> x.Id);
        builder.OwnsOne(x => x.Price).Property(x => x.Currency);
        builder.OwnsOne(x => x.Price).Property(x => x.Value);
    }
}
