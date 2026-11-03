using IgloReservation.Api.Models.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace IgloReservation.Api.Data.Configurations;

public class ReservationConfiguration : IEntityTypeConfiguration<Reservation>
{
    public void Configure(EntityTypeBuilder<Reservation> builder)
    {
        builder.ToTable("reservations");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.UserId)
            .IsRequired()
            .HasMaxLength(450);

        builder.Property(x => x.StartAtUtc)
            .IsRequired()
            .HasColumnType("timestamp with time zone");

        builder.Property(x => x.EndAtUtc)
            .IsRequired()
            .HasColumnType("timestamp with time zone");

        builder.Property(x => x.Status)
            .IsRequired()
            .HasConversion<string>()
            .HasMaxLength(32);

        builder.Property(x => x.Description)
            .HasMaxLength(1000);

        builder.Property(x => x.CreatedAtUtc)
            .IsRequired()
            .HasColumnType("timestamp with time zone");

        builder.Property(x => x.DeletedAtUtc)
            .HasColumnType("timestamp with time zone");

        builder.Property(x => x.IsDeleted)
            .IsRequired();

        builder.HasOne(x => x.User)
            .WithMany(u => u.Reservations)
            .HasForeignKey(x => x.UserId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.IgloUnit)
            .WithMany(u => u.Reservations)
            .HasForeignKey(x => x.IgloUnitId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(x => x.UserId);
        builder.HasIndex(x => x.Status);
        builder.HasIndex(x => x.IsDeleted);
        builder.HasIndex(x => new { x.IgloUnitId, x.StartAtUtc, x.EndAtUtc });

        // Active overlap is also enforced in DB via GiST exclusion (see InitialCreate SQL).

        builder.HasQueryFilter(x => !x.IsDeleted);

        builder.ToTable(t =>
            t.HasCheckConstraint(
                "ck_reservations_end_after_start",
                "\"EndAtUtc\" > \"StartAtUtc\""));
    }
}
