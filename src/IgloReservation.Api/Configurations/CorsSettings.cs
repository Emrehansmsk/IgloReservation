namespace IgloReservation.Api.Configurations;

/// <summary>
/// CORS allowed origins. Populate with approved frontend URLs (still an open plan item).
/// </summary>
public class CorsSettings
{
    public const string SectionName = "Cors";

    public string[] AllowedOrigins { get; set; } = [];
}
