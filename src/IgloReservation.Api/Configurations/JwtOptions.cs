namespace IgloReservation.Api.Configurations;

/// <summary>
/// JWT options. Signing key must come from User Secrets / environment — never commit secrets.
/// </summary>
public class JwtOptions
{
    public const string SectionName = "Jwt";

    public string Issuer { get; set; } = string.Empty;
    public string Audience { get; set; } = string.Empty;

    /// <summary>
    /// Bound from configuration only for local wiring. Prefer env var Jwt__SigningKey / User Secrets.
    /// </summary>
    public string SigningKey { get; set; } = string.Empty;

    public int AccessTokenMinutes { get; set; } = 60;
}
