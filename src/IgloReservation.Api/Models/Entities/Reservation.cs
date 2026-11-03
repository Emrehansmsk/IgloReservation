namespace IgloReservation.Api.Models.Entities;

/// <summary>
/// Reservation. <see cref="StartAtUtc"/> / <see cref="EndAtUtc"/> are stored in UTC.
/// Business rules and display use Europe/Istanbul (see BusinessTimezone).
/// Free-text details (guest, deposit, notes) live in <see cref="Description"/>.
/// </summary>
public class Reservation : ISoftDeletable
{
    public Guid Id { get; set; }

    public string UserId { get; set; } = string.Empty;
    public ApplicationUser? User { get; set; }

    public Guid IgloUnitId { get; set; }
    public IgloUnit? IgloUnit { get; set; }

    public DateTimeOffset StartAtUtc { get; set; }
    public DateTimeOffset EndAtUtc { get; set; }

    public ReservationStatus Status { get; set; } = ReservationStatus.Pending;

    /// <summary>
    /// Free-form board cell text (guest, party size, deposit, staff, etc.).
    /// </summary>
    public string? Description { get; set; }

    public DateTimeOffset CreatedAtUtc { get; set; }

    public bool IsDeleted { get; set; }
    public DateTimeOffset? DeletedAtUtc { get; set; }
}
