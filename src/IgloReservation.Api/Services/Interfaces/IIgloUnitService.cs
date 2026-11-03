using IgloReservation.Api.DTOs.Common;
using IgloReservation.Api.DTOs.IgloUnits;

namespace IgloReservation.Api.Services.Interfaces;

public interface IIgloUnitService
{
    Task<PagedResult<IgloUnitResponse>> ListAsync(int page, int pageSize, CancellationToken cancellationToken = default);

    Task<IgloUnitResponse?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    Task<IgloUnitResponse> CreateAsync(CreateIgloUnitRequest request, CancellationToken cancellationToken = default);

    Task<IgloUnitResponse?> UpdateAsync(Guid id, UpdateIgloUnitRequest request, CancellationToken cancellationToken = default);

    Task<bool> SoftDeleteAsync(Guid id, CancellationToken cancellationToken = default);
}
