using IgloReservation.Api.Exceptions;
using IgloReservation.Api.Services;
using IgloReservation.Api.Services.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;

namespace IgloReservation.Api.Tests;

public class AdminBootstrapHostedServiceTests
{
    [Fact]
    public async Task StartAsync_RethrowsAdminBootstrapException()
    {
        var bootstrap = new Mock<IAdminBootstrapService>();
        bootstrap
            .Setup(b => b.EnsureAdminAsync(It.IsAny<CancellationToken>()))
            .ThrowsAsync(new AdminBootstrapException("AdminBootstrap is misconfigured: test"));

        var services = new ServiceCollection();
        services.AddSingleton(bootstrap.Object);
        await using var provider = services.BuildServiceProvider();
        var scopeFactory = provider.GetRequiredService<IServiceScopeFactory>();

        var hosted = new AdminBootstrapHostedService(scopeFactory, NullLogger<AdminBootstrapHostedService>.Instance);

        await Assert.ThrowsAsync<AdminBootstrapException>(() => hosted.StartAsync(CancellationToken.None));
    }

    [Fact]
    public async Task StartAsync_SwallowsNonBootstrapFailures()
    {
        var bootstrap = new Mock<IAdminBootstrapService>();
        bootstrap
            .Setup(b => b.EnsureAdminAsync(It.IsAny<CancellationToken>()))
            .ThrowsAsync(new TimeoutException("database unreachable"));

        var services = new ServiceCollection();
        services.AddSingleton(bootstrap.Object);
        await using var provider = services.BuildServiceProvider();
        var scopeFactory = provider.GetRequiredService<IServiceScopeFactory>();

        var hosted = new AdminBootstrapHostedService(scopeFactory, NullLogger<AdminBootstrapHostedService>.Instance);

        await hosted.StartAsync(CancellationToken.None);
    }
}
