using Domain.Guest.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Data.MySql.Configuration;

public class GuestConfiguration : IEntityTypeConfiguration<Guest>
{
    public void Configure (EntityTypeBuilder<Guest> builder)
    {
        builder.HasKey(x => x.Id);
        builder.OwnsOne(x => x.DocumentId).Property(x => x.IdNumber);

        builder.OwnsOne(x => x.DocumentId).Property(x => x.Document);
    }
}
