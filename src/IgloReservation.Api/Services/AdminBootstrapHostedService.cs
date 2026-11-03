using IgloReservation.Api.Exceptions;
using IgloReservation.Api.Services.Interfaces;

namespace IgloReservation.Api.Services;

/// <summary>
/// Startup hook for admin bootstrap. Does not run migrations.
/// <see cref="AdminBootstrapException"/> fails startup. Database unavailability is logged without
/// secrets so local auth-pipeline tests can still start without PostgreSQL.
/// </summary>
public class AdminBootstrapHostedService : IHostedService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<AdminBootstrapHostedService> _logger;

    public AdminBootstrapHostedService(
        IServiceScopeFactory scopeFactory,
        ILogger<AdminBootstrapHostedService> logger)
    {
        _scopeFactory = scopeFactory;
        _logger = logger;
    }

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        try
        {
            using var scope = _scopeFactory.CreateScope();
            var bootstrap = scope.ServiceProvider.GetRequiredService<IAdminBootstrapService>();
            await bootstrap.EnsureAdminAsync(cancellationToken);
        }
        catch (AdminBootstrapException ex)
        {
            _logger.LogError("Admin bootstrap failed: {Message}", ex.Message);
            throw;
        }
        catch (Exception ex)
        {
            // Avoid logging full exception graphs (may include connection details).
            _logger.LogWarning(
                "Admin bootstrap did not complete ({ExceptionType}). App will continue; ensure PostgreSQL is reachable and migrations are applied.",
                ex.GetType().Name);
        }
    }

    public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;
}
