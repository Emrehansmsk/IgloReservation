using System.ComponentModel.DataAnnotations;
using IgloReservation.Api.Models.Entities;

namespace IgloReservation.Api.DTOs.Reservations;

public class CreateReservationRequest : IValidatableObject
{
    [Required, MaxLength(450)]
    public string UserId { get; set; } = string.Empty;

    [Required]
    public Guid IgloUnitId { get; set; }

    [Required]
    public DateTimeOffset StartAtUtc { get; set; }

    [Required]
    public DateTimeOffset EndAtUtc { get; set; }

    [EnumDataType(typeof(ReservationStatus))]
    public ReservationStatus Status { get; set; } = ReservationStatus.Pending;

    [MaxLength(1000)]
    public string? Description { get; set; }

    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        if (IgloUnitId == Guid.Empty)
        {
            yield return new ValidationResult("IgloUnitId is required.", [nameof(IgloUnitId)]);
        }

        if (EndAtUtc <= StartAtUtc)
        {
            yield return new ValidationResult("EndAtUtc must be after StartAtUtc.", [nameof(EndAtUtc), nameof(StartAtUtc)]);
        }
    }
}
