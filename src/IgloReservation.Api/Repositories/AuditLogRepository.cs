using IgloReservation.Api.Data;
using IgloReservation.Api.Models.Entities;
using IgloReservation.Api.Repositories.Interfaces;

namespace IgloReservation.Api.Repositories;

public class AuditLogRepository : IAuditLogRepository
{
    private readonly ApplicationDbContext _db;

    public AuditLogRepository(ApplicationDbContext db)
    {
        _db = db;
    }

    public async Task AddAsync(AuditLog entry, CancellationToken cancellationToken = default)
    {
        await _db.AuditLogs.AddAsync(entry, cancellationToken);
        await _db.SaveChangesAsync(cancellationToken);
    }
}
