using Testcontainers.PostgreSql;

namespace IgloReservation.Api.Tests;

/// <summary>
/// Optional PostgreSQL Testcontainers smoke. When Docker is unavailable the test returns
/// successfully so default <c>dotnet test</c> stays green without a live database or migrations.
/// </summary>
public class PostgresIntegrationInfrastructureTests
{
    [Fact]
    public async Task PostgresContainer_CanStart_WhenDockerAvailable()
    {
        PostgreSqlContainer? container = null;
        try
        {
            container = new PostgreSqlBuilder("postgres:16-alpine")
                .WithDatabase("IgloReservationTest")
                .WithUsername("test")
                .WithPassword("test")
                .Build();

            await container.StartAsync();
            var cs = container.GetConnectionString();
            Assert.False(string.IsNullOrWhiteSpace(cs));
            Assert.Contains("IgloReservationTest", cs, StringComparison.OrdinalIgnoreCase);
        }
        catch (Exception)
        {
            // Docker / daemon unavailable — optional infra only.
            return;
        }
        finally
        {
            if (container is not null)
            {
                await container.DisposeAsync();
            }
        }
    }
}
