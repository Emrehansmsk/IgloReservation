using IgloReservation.Api.Data;
using IgloReservation.Api.Models.Entities;
using IgloReservation.Api.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace IgloReservation.Api.Repositories;

public class ReservationRepository : IReservationRepository
{
    private readonly ApplicationDbContext _db;

    public ReservationRepository(ApplicationDbContext db)
    {
        _db = db;
    }

    public Task<Reservation?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default) =>
        _db.Reservations.FirstOrDefaultAsync(x => x.Id == id, cancellationToken);

    public async Task<(IReadOnlyList<Reservation> Items, int TotalCount)> ListAsync(
        int page,
        int pageSize,
        string? ownerUserId,
        CancellationToken cancellationToken = default)
    {
        var query = _db.Reservations.AsNoTracking().AsQueryable();

        if (!string.IsNullOrWhiteSpace(ownerUserId))
        {
            query = query.Where(x => x.UserId == ownerUserId);
        }

        query = query.OrderByDescending(x => x.StartAtUtc);
        var total = await query.CountAsync(cancellationToken);
        var items = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);
        return (items, total);
    }

    public async Task AddAsync(Reservation reservation, CancellationToken cancellationToken = default)
    {
        await _db.Reservations.AddAsync(reservation, cancellationToken);
        await _db.SaveChangesAsync(cancellationToken);
    }

    public async Task UpdateAsync(Reservation reservation, CancellationToken cancellationToken = default)
    {
        _db.Reservations.Update(reservation);
        await _db.SaveChangesAsync(cancellationToken);
    }

    public async Task SoftDeleteAsync(Reservation reservation, CancellationToken cancellationToken = default)
    {
        reservation.IsDeleted = true;
        reservation.DeletedAtUtc = DateTimeOffset.UtcNow;
        _db.Reservations.Update(reservation);
        await _db.SaveChangesAsync(cancellationToken);
    }

    public Task<bool> HasOverlappingActiveAsync(
        Guid igloUnitId,
        DateTimeOffset startAtUtc,
        DateTimeOffset endAtUtc,
        Guid? excludeReservationId,
        CancellationToken cancellationToken = default)
    {
        // Overlap: existing.Start < new.End AND existing.End > new.Start
        // Active for conflict: Pending | Confirmed. Soft-deleted rows filtered by global query filter.
        // DB also enforces this via ex_reservations_active_overlap (GiST exclusion).
        var query = _db.Reservations.AsNoTracking()
            .Where(r => r.IgloUnitId == igloUnitId)
            .Where(r => r.Status == ReservationStatus.Pending || r.Status == ReservationStatus.Confirmed)
            .Where(r => r.StartAtUtc < endAtUtc && r.EndAtUtc > startAtUtc);

        if (excludeReservationId.HasValue)
        {
            query = query.Where(r => r.Id != excludeReservationId.Value);
        }

        return query.AnyAsync(cancellationToken);
    }
}
