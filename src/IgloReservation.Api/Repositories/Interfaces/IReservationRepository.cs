using IgloReservation.Api.Models.Entities;

namespace IgloReservation.Api.Repositories.Interfaces;

public interface IReservationRepository
{
    Task<Reservation?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    /// <param name="ownerUserId">When set, only reservations owned by this user are returned.</param>
    Task<(IReadOnlyList<Reservation> Items, int TotalCount)> ListAsync(
        int page,
        int pageSize,
        string? ownerUserId,
        CancellationToken cancellationToken = default);

    Task AddAsync(Reservation reservation, CancellationToken cancellationToken = default);

    Task UpdateAsync(Reservation reservation, CancellationToken cancellationToken = default);

    Task SoftDeleteAsync(Reservation reservation, CancellationToken cancellationToken = default);

    /// <summary>
    /// Application-level overlap probe. Must be paired with DB exclusion/locking — service-only is not enough.
    /// Active statuses for conflict: Pending, Confirmed.
    /// </summary>
    Task<bool> HasOverlappingActiveAsync(
        Guid igloUnitId,
        DateTimeOffset startAtUtc,
        DateTimeOffset endAtUtc,
        Guid? excludeReservationId,
        CancellationToken cancellationToken = default);
}
