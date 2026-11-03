namespace IgloReservation.Api.Models.Entities;

/// <summary>
/// Soft-delete contract. Domain records are not hard-deleted (locked plan).
/// </summary>
public interface ISoftDeletable
{
    bool IsDeleted { get; set; }
    DateTimeOffset? DeletedAtUtc { get; set; }
}
