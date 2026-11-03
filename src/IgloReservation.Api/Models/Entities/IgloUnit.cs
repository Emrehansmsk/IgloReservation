namespace IgloReservation.Api.Models.Entities;

/// <summary>
/// Bookable iglo / unit. Extra fields (capacity, description, sort) are open plan items — not invented here.
/// </summary>
public class IgloUnit : ISoftDeletable
{
    public Guid Id { get; set; }

    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Soft deactivate for availability without removing the row.
    /// </summary>
    public bool IsActive { get; set; } = true;

    public bool IsDeleted { get; set; }
    public DateTimeOffset? DeletedAtUtc { get; set; }

    public DateTimeOffset CreatedAtUtc { get; set; }

    public ICollection<Reservation> Reservations { get; set; } = new List<Reservation>();
}
