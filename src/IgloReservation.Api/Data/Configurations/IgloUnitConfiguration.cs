using IgloReservation.Api.Models.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace IgloReservation.Api.Data.Configurations;

public class IgloUnitConfiguration : IEntityTypeConfiguration<IgloUnit>
{
    public void Configure(EntityTypeBuilder<IgloUnit> builder)
    {
        builder.ToTable("iglo_units");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Name)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(x => x.IsActive)
            .IsRequired();

        builder.Property(x => x.IsDeleted)
            .IsRequired();

        builder.Property(x => x.CreatedAtUtc)
            .IsRequired()
            .HasColumnType("timestamp with time zone");

        builder.Property(x => x.DeletedAtUtc)
            .HasColumnType("timestamp with time zone");

        builder.HasIndex(x => x.IsDeleted);
        builder.HasIndex(x => x.IsActive);

        builder.HasQueryFilter(x => !x.IsDeleted);
    }
}
