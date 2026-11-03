namespace IgloReservation.Api.Exceptions;

public sealed class ReservationConflictException : Exception
{
    public ReservationConflictException(string message) : base(message)
    {
    }
}
