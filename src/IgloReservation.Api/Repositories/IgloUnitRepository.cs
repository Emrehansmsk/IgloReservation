using IgloReservation.Api.Data;
using IgloReservation.Api.Models.Entities;
using IgloReservation.Api.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace IgloReservation.Api.Repositories;

public class IgloUnitRepository : IIgloUnitRepository
{
    private readonly ApplicationDbContext _db;

    public IgloUnitRepository(ApplicationDbContext db)
    {
        _db = db;
    }

    public Task<IgloUnit?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default) =>
        _db.IgloUnits.FirstOrDefaultAsync(x => x.Id == id, cancellationToken);

    public async Task<(IReadOnlyList<IgloUnit> Items, int TotalCount)> ListAsync(
        int page,
        int pageSize,
        CancellationToken cancellationToken = default)
    {
        // Oldest first so newly created units appear at the end of the board.
        var query = _db.IgloUnits.AsNoTracking()
            .OrderBy(x => x.CreatedAtUtc)
            .ThenBy(x => x.Name);
        var total = await query.CountAsync(cancellationToken);
        var items = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);
        return (items, total);
    }

    public async Task AddAsync(IgloUnit unit, CancellationToken cancellationToken = default)
    {
        await _db.IgloUnits.AddAsync(unit, cancellationToken);
        await _db.SaveChangesAsync(cancellationToken);
    }

    public async Task UpdateAsync(IgloUnit unit, CancellationToken cancellationToken = default)
    {
        _db.IgloUnits.Update(unit);
        await _db.SaveChangesAsync(cancellationToken);
    }

    public async Task SoftDeleteAsync(IgloUnit unit, CancellationToken cancellationToken = default)
    {
        unit.IsDeleted = true;
        unit.DeletedAtUtc = DateTimeOffset.UtcNow;
        unit.IsActive = false;
        _db.IgloUnits.Update(unit);
        await _db.SaveChangesAsync(cancellationToken);
    }
}
