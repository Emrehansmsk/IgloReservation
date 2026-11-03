namespace IgloReservation.Api.Models.Entities;

/// <summary>
/// Admin action audit trail. Do not log passwords, tokens, or unnecessary PII.
/// Scope of which writes are audited remains an open plan item.
/// </summary>
public class AuditLog
{
    public Guid Id { get; set; }

    public string? ActorUserId { get; set; }
    public ApplicationUser? ActorUser { get; set; }

    public string Action { get; set; } = string.Empty;
    public string EntityType { get; set; } = string.Empty;
    public string EntityId { get; set; } = string.Empty;

    public DateTimeOffset OccurredAtUtc { get; set; }
}
