using IgloReservation.Api.Models.Entities;

namespace IgloReservation.Api.Repositories.Interfaces;

public interface IIgloUnitRepository
{
    Task<IgloUnit?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    Task<(IReadOnlyList<IgloUnit> Items, int TotalCount)> ListAsync(
        int page,
        int pageSize,
        CancellationToken cancellationToken = default);

    Task AddAsync(IgloUnit unit, CancellationToken cancellationToken = default);

    Task UpdateAsync(IgloUnit unit, CancellationToken cancellationToken = default);

    Task SoftDeleteAsync(IgloUnit unit, CancellationToken cancellationToken = default);
}
