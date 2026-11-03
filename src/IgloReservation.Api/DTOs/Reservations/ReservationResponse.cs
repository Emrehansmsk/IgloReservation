using IgloReservation.Api.Models.Entities;

namespace IgloReservation.Api.DTOs.Reservations;

public class ReservationResponse
{
    public Guid Id { get; set; }
    public string UserId { get; set; } = string.Empty;
    public Guid IgloUnitId { get; set; }
    public DateTimeOffset StartAtUtc { get; set; }
    public DateTimeOffset EndAtUtc { get; set; }
    public ReservationStatus Status { get; set; }
    public string? Description { get; set; }
    public DateTimeOffset CreatedAtUtc { get; set; }
}
