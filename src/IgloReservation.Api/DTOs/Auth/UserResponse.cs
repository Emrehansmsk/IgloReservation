namespace IgloReservation.Api.DTOs.Auth;

/// <summary>
/// Public user shape — never includes password hash, security stamp, or secrets.
/// </summary>
public class UserResponse
{
    public string Id { get; set; } = string.Empty;
    public string? Email { get; set; }
    public string? UserName { get; set; }
    public IReadOnlyList<string> Roles { get; set; } = [];
}
