using AutoMapper;
using IgloReservation.Api.DTOs.Common;
using IgloReservation.Api.DTOs.IgloUnits;
using IgloReservation.Api.Models.Entities;
using IgloReservation.Api.Repositories.Interfaces;
using IgloReservation.Api.Services.Interfaces;

namespace IgloReservation.Api.Services;

public class IgloUnitService : IIgloUnitService
{
    private readonly IIgloUnitRepository _units;
    private readonly IAuditLogRepository _auditLogs;
    private readonly IMapper _mapper;

    public IgloUnitService(
        IIgloUnitRepository units,
        IAuditLogRepository auditLogs,
        IMapper mapper)
    {
        _units = units;
        _auditLogs = auditLogs;
        _mapper = mapper;
    }

    public async Task<PagedResult<IgloUnitResponse>> ListAsync(
        int page,
        int pageSize,
        CancellationToken cancellationToken = default)
    {
        page = page < 1 ? 1 : page;
        pageSize = pageSize < 1 ? 20 : Math.Min(pageSize, 100);

        var (items, total) = await _units.ListAsync(page, pageSize, cancellationToken);
        return new PagedResult<IgloUnitResponse>
        {
            Items = _mapper.Map<IReadOnlyList<IgloUnitResponse>>(items),
            Page = page,
            PageSize = pageSize,
            TotalCount = total
        };
    }

    public async Task<IgloUnitResponse?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var entity = await _units.GetByIdAsync(id, cancellationToken);
        return entity is null ? null : _mapper.Map<IgloUnitResponse>(entity);
    }

    public async Task<IgloUnitResponse> CreateAsync(
        CreateIgloUnitRequest request,
        CancellationToken cancellationToken = default)
    {
        var entity = _mapper.Map<IgloUnit>(request);
        entity.Id = Guid.NewGuid();
        entity.CreatedAtUtc = DateTimeOffset.UtcNow;

        await _units.AddAsync(entity, cancellationToken);
        return _mapper.Map<IgloUnitResponse>(entity);
    }

    public async Task<IgloUnitResponse?> UpdateAsync(
        Guid id,
        UpdateIgloUnitRequest request,
        CancellationToken cancellationToken = default)
    {
        var entity = await _units.GetByIdAsync(id, cancellationToken);
        if (entity is null)
        {
            return null;
        }

        _mapper.Map(request, entity);
        await _units.UpdateAsync(entity, cancellationToken);
        return _mapper.Map<IgloUnitResponse>(entity);
    }

    public async Task<bool> SoftDeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var entity = await _units.GetByIdAsync(id, cancellationToken);
        if (entity is null)
        {
            return false;
        }

        await _units.SoftDeleteAsync(entity, cancellationToken);

        await _auditLogs.AddAsync(new AuditLog
        {
            Id = Guid.NewGuid(),
            ActorUserId = null,
            Action = "SoftDelete",
            EntityType = nameof(IgloUnit),
            EntityId = id.ToString(),
            OccurredAtUtc = DateTimeOffset.UtcNow
        }, cancellationToken);

        return true;
    }
}
