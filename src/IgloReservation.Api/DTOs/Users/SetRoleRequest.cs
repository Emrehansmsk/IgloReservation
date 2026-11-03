using System.ComponentModel.DataAnnotations;

namespace IgloReservation.Api.DTOs.Users;

public class SetRoleRequest
{
    [Required, MaxLength(32)]
    public string Role { get; set; } = string.Empty;
}
