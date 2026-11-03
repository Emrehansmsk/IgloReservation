using Microsoft.AspNetCore.Identity;

namespace IgloReservation.Api.Models.Entities;

/// <summary>
/// Identity user. Activation policy (custom IsActive vs lockout) remains an open plan item.
/// Roles: Admin, User — assigned via Identity, never invented on the client.
/// </summary>
public class ApplicationUser : IdentityUser
{
    public ICollection<Reservation> Reservations { get; set; } = new List<Reservation>();
    public ICollection<AuditLog> AuditLogs { get; set; } = new List<AuditLog>();
}
