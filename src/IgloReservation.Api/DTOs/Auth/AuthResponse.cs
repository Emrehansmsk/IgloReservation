namespace IgloReservation.Api.DTOs.Auth;

/// <summary>
/// v1 access-token only — no refresh token.
/// </summary>
public class AuthResponse
{
    public string AccessToken { get; set; } = string.Empty;
    public DateTimeOffset ExpiresAtUtc { get; set; }
    public UserResponse User { get; set; } = new();
}
