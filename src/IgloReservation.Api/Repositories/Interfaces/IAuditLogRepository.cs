using IgloReservation.Api.Models.Entities;

namespace IgloReservation.Api.Repositories.Interfaces;

public interface IAuditLogRepository
{
    Task AddAsync(AuditLog entry, CancellationToken cancellationToken = default);
}
