using System.ComponentModel.DataAnnotations;

namespace IgloReservation.Api.DTOs.Auth;

/// <summary>
/// Public registration. Any client-supplied role is ignored; new accounts always get User.
/// </summary>
public class RegisterRequest
{
    [Required, EmailAddress, MaxLength(256)]
    public string Email { get; set; } = string.Empty;

    [Required, MinLength(8), MaxLength(128)]
    public string Password { get; set; } = string.Empty;

    [MaxLength(256)]
    public string? UserName { get; set; }
}
