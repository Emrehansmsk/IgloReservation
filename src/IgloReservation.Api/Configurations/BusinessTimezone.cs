namespace IgloReservation.Api.Configurations;

/// <summary>
/// Locked business timezone. Persist instants as UTC in PostgreSQL (timestamptz);
/// convert for business rules / display using <see cref="IanaId"/>.
/// </summary>
public static class BusinessTimezone
{
    public const string IanaId = "Europe/Istanbul";
}
