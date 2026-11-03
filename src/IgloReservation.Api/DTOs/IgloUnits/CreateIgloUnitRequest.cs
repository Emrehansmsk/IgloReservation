using System.ComponentModel.DataAnnotations;

namespace IgloReservation.Api.DTOs.IgloUnits;

public class CreateIgloUnitRequest
{
    [Required, MinLength(1), MaxLength(200)]
    public string Name { get; set; } = string.Empty;

    public bool IsActive { get; set; } = true;
}
