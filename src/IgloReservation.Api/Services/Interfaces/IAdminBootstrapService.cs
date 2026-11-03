namespace IgloReservation.Api.Services.Interfaces;

public interface IAdminBootstrapService
{
    /// <summary>
    /// Ensures Identity roles exist. When AdminBootstrap email/password are both supplied via
    /// User Secrets / environment, creates the Admin user if missing. Idempotent for existing users
    /// (does not change password or roles). Throws on partial/invalid configuration.
    /// </summary>
    Task EnsureAdminAsync(CancellationToken cancellationToken = default);
}
