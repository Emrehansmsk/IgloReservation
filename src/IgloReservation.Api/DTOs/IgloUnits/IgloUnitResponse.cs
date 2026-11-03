namespace IgloReservation.Api.DTOs.IgloUnits;

public class IgloUnitResponse
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public bool IsActive { get; set; }
    public DateTimeOffset CreatedAtUtc { get; set; }
}
