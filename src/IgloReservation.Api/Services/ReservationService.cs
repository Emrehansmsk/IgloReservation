using AutoMapper;
using IgloReservation.Api.DTOs.Common;
using IgloReservation.Api.DTOs.Reservations;
using IgloReservation.Api.Exceptions;
using IgloReservation.Api.Models.Entities;
using IgloReservation.Api.Repositories.Interfaces;
using IgloReservation.Api.Services.Interfaces;

namespace IgloReservation.Api.Services;

public class ReservationService : IReservationService
{
    /// <summary>
    /// Past-date policy (sensible default): EndAtUtc must be in the future; StartAtUtc may be at most
    /// <see cref="StartGrace"/> in the past (clock skew / late check-in). Business TZ Europe/Istanbul
    /// is used for display elsewhere; comparisons here use UTC instants.
    /// </summary>
    public static readonly TimeSpan StartGrace = TimeSpan.FromMinutes(15);

    private readonly IReservationRepository _reservations;
    private readonly IAuditLogRepository _auditLogs;
    private readonly IMapper _mapper;

    public ReservationService(
        IReservationRepository reservations,
        IAuditLogRepository auditLogs,
        IMapper mapper)
    {
        _reservations = reservations;
        _auditLogs = auditLogs;
        _mapper = mapper;
    }

    public async Task<PagedResult<ReservationResponse>> ListAsync(
        string requesterUserId,
        bool isAdmin,
        int page,
        int pageSize,
        CancellationToken cancellationToken = default)
    {
        page = page < 1 ? 1 : page;
        pageSize = pageSize < 1 ? 20 : Math.Min(pageSize, 100);

        // Board visibility: Admin and User both see all reservations (writes remain Admin-only).
        _ = requesterUserId;
        _ = isAdmin;
        var (items, total) = await _reservations.ListAsync(page, pageSize, ownerUserId: null, cancellationToken);
        return new PagedResult<ReservationResponse>
        {
            Items = _mapper.Map<IReadOnlyList<ReservationResponse>>(items),
            Page = page,
            PageSize = pageSize,
            TotalCount = total
        };
    }

    public async Task<ReservationResponse?> GetByIdAsync(
        Guid id,
        string requesterUserId,
        bool isAdmin,
        CancellationToken cancellationToken = default)
    {
        _ = requesterUserId;
        _ = isAdmin;

        var entity = await _reservations.GetByIdAsync(id, cancellationToken);
        return entity is null ? null : _mapper.Map<ReservationResponse>(entity);
    }

    public async Task<ReservationResponse> CreateAsync(
        CreateReservationRequest request,
        CancellationToken cancellationToken = default)
    {
        ValidateSchedule(request.StartAtUtc, request.EndAtUtc, enforceFuturePolicy: true);
        await EnsureNoActiveOverlapAsync(
            request.IgloUnitId,
            request.StartAtUtc,
            request.EndAtUtc,
            excludeReservationId: null,
            cancellationToken);

        var entity = _mapper.Map<Reservation>(request);
        entity.Id = Guid.NewGuid();
        entity.CreatedAtUtc = DateTimeOffset.UtcNow;

        await _reservations.AddAsync(entity, cancellationToken);
        return _mapper.Map<ReservationResponse>(entity);
    }

    public async Task<ReservationResponse?> UpdateAsync(
        Guid id,
        UpdateReservationRequest request,
        CancellationToken cancellationToken = default)
    {
        var entity = await _reservations.GetByIdAsync(id, cancellationToken);
        if (entity is null)
        {
            return null;
        }

        // Updates that move the window into the past entirely are rejected; Completed/Cancelled may keep past ranges
        // only when status is not becoming/staying active for conflict.
        var enforceFuture = request.Status is ReservationStatus.Pending or ReservationStatus.Confirmed;
        ValidateSchedule(request.StartAtUtc, request.EndAtUtc, enforceFuturePolicy: enforceFuture);
        await EnsureNoActiveOverlapAsync(
            request.IgloUnitId,
            request.StartAtUtc,
            request.EndAtUtc,
            excludeReservationId: id,
            cancellationToken);

        _mapper.Map(request, entity);
        await _reservations.UpdateAsync(entity, cancellationToken);
        return _mapper.Map<ReservationResponse>(entity);
    }

    public async Task<bool> SoftDeleteAsync(
        Guid id,
        string? actorUserId,
        CancellationToken cancellationToken = default)
    {
        var entity = await _reservations.GetByIdAsync(id, cancellationToken);
        if (entity is null)
        {
            return false;
        }

        await _reservations.SoftDeleteAsync(entity, cancellationToken);

        await _auditLogs.AddAsync(new AuditLog
        {
            Id = Guid.NewGuid(),
            ActorUserId = actorUserId,
            Action = "SoftDelete",
            EntityType = nameof(Reservation),
            EntityId = id.ToString(),
            OccurredAtUtc = DateTimeOffset.UtcNow
        }, cancellationToken);

        return true;
    }

    public async Task EnsureNoActiveOverlapAsync(
        Guid igloUnitId,
        DateTimeOffset startAtUtc,
        DateTimeOffset endAtUtc,
        Guid? excludeReservationId,
        CancellationToken cancellationToken = default)
    {
        var overlaps = await _reservations.HasOverlappingActiveAsync(
            igloUnitId,
            startAtUtc,
            endAtUtc,
            excludeReservationId,
            cancellationToken);

        if (overlaps)
        {
            throw new ReservationConflictException(
                "An active reservation (Pending or Confirmed) overlaps this time range for the unit.");
        }
    }

    public static void ValidateSchedule(DateTimeOffset startAtUtc, DateTimeOffset endAtUtc, bool enforceFuturePolicy)
    {
        if (endAtUtc <= startAtUtc)
        {
            throw new BusinessRuleException("EndAtUtc must be after StartAtUtc.");
        }

        if (!enforceFuturePolicy)
        {
            return;
        }

        var now = DateTimeOffset.UtcNow;
        if (endAtUtc <= now)
        {
            throw new BusinessRuleException("EndAtUtc must be in the future.");
        }

        if (startAtUtc < now - StartGrace)
        {
            throw new BusinessRuleException(
                $"StartAtUtc cannot be more than {StartGrace.TotalMinutes:0} minutes in the past.");
        }
    }
}
