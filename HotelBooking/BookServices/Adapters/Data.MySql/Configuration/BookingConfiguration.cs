using Domain.Book.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Data.MySql.Configuration;

public class BookingConfiguration : IEntityTypeConfiguration<Booking>
{
    public void Configure(EntityTypeBuilder<Booking> builder)
    {
        builder.ToTable("Book");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.PlacedAt).IsRequired();
        builder.Property(x => x.Start).IsRequired();
        builder.Property(x => x.End).IsRequired();

        builder.Property(x => x.Status)
            .HasConversion<int>()
            .IsRequired();

        builder.Ignore(x => x.CurrentStatus);

        builder.Property(x => x.RoomId).IsRequired();
        builder.Property(x => x.GuestId).IsRequired();

        builder.HasOne(x => x.Room)
            .WithMany(x => x.Bookings)
            .HasForeignKey(x => x.RoomId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.Guest)
            .WithMany(x => x.Bookings)
            .HasForeignKey(x => x.GuestId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}