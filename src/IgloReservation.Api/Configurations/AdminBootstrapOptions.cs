namespace IgloReservation.Api.Configurations;

/// <summary>
/// First Admin credentials. Bind only from environment / User Secrets — never from committed appsettings passwords.
/// </summary>
public class AdminBootstrapOptions
{
    public const string SectionName = "AdminBootstrap";

    public string? Email { get; set; }
    public string? Password { get; set; }

    public bool HasAnyValue =>
        !string.IsNullOrWhiteSpace(Email) || !string.IsNullOrWhiteSpace(Password);

    public bool HasCompleteCredentials =>
        !string.IsNullOrWhiteSpace(Email) && !string.IsNullOrWhiteSpace(Password);
}
