using IgloReservation.Api.DTOs.Common;
using IgloReservation.Api.DTOs.Reservations;

namespace IgloReservation.Api.Services.Interfaces;

public interface IReservationService
{
    /// <param name="requesterUserId">Authenticated user id (reserved for future use).</param>
    /// <param name="isAdmin">True when caller is Admin (writes stay Admin-only; reads are shared).</param>
    Task<PagedResult<ReservationResponse>> ListAsync(
        string requesterUserId,
        bool isAdmin,
        int page,
        int pageSize,
        CancellationToken cancellationToken = default);

    Task<ReservationResponse?> GetByIdAsync(
        Guid id,
        string requesterUserId,
        bool isAdmin,
        CancellationToken cancellationToken = default);

    Task<ReservationResponse> CreateAsync(CreateReservationRequest request, CancellationToken cancellationToken = default);

    Task<ReservationResponse?> UpdateAsync(Guid id, UpdateReservationRequest request, CancellationToken cancellationToken = default);

    Task<bool> SoftDeleteAsync(Guid id, string? actorUserId, CancellationToken cancellationToken = default);

    Task EnsureNoActiveOverlapAsync(
        Guid igloUnitId,
        DateTimeOffset startAtUtc,
        DateTimeOffset endAtUtc,
        Guid? excludeReservationId,
        CancellationToken cancellationToken = default);
}
