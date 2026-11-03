namespace IgloReservation.Api.Models.Entities;

/// <summary>
/// Locked status set. Overlap treats <see cref="Pending"/> and <see cref="Confirmed"/> as active;
/// <see cref="Cancelled"/> and <see cref="Completed"/> are excluded from conflict checks.
/// </summary>
public enum ReservationStatus
{
    Pending = 0,
    Confirmed = 1,
    Cancelled = 2,
    Completed = 3
}

public static class ReservationStatusExtensions
{
    public static bool IsActiveForConflict(this ReservationStatus status) =>
        status is ReservationStatus.Pending or ReservationStatus.Confirmed;
}
