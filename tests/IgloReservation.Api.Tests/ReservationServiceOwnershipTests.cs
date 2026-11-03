using IgloReservation.Api.Models.Entities;
using IgloReservation.Api.Repositories.Interfaces;
using IgloReservation.Api.Services;
using Moq;

namespace IgloReservation.Api.Tests;

public class ReservationServiceOwnershipTests
{
    private readonly Mock<IReservationRepository> _reservations = new();
    private readonly Mock<IAuditLogRepository> _audit = new();
    private readonly ReservationService _sut;

    public ReservationServiceOwnershipTests()
    {
        _sut = new ReservationService(_reservations.Object, _audit.Object, TestMapper.Create());
    }

    [Fact]
    public async Task ListAsync_AsUser_DoesNotFilterByOwner()
    {
        _reservations
            .Setup(r => r.ListAsync(1, 20, null, It.IsAny<CancellationToken>()))
            .ReturnsAsync((new List<Reservation>
            {
                new()
                {
                    Id = Guid.NewGuid(),
                    UserId = "admin-owner",
                    IgloUnitId = Guid.NewGuid(),
                    StartAtUtc = DateTimeOffset.UtcNow.AddDays(1),
                    EndAtUtc = DateTimeOffset.UtcNow.AddDays(2),
                    Status = ReservationStatus.Pending,
                    CreatedAtUtc = DateTimeOffset.UtcNow
                }
            }, 1));

        var result = await _sut.ListAsync("user-1", isAdmin: false, page: 1, pageSize: 20);

        Assert.Single(result.Items);
        _reservations.Verify(r => r.ListAsync(1, 20, null, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task ListAsync_AsAdmin_DoesNotFilterByOwner()
    {
        _reservations
            .Setup(r => r.ListAsync(1, 20, null, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Array.Empty<Reservation>(), 0));

        await _sut.ListAsync("admin-1", isAdmin: true, page: 1, pageSize: 20);

        _reservations.Verify(r => r.ListAsync(1, 20, null, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task GetByIdAsync_AsUser_ReturnsOtherUsersReservation()
    {
        var id = Guid.NewGuid();
        _reservations.Setup(r => r.GetByIdAsync(id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new Reservation
            {
                Id = id,
                UserId = "owner",
                IgloUnitId = Guid.NewGuid(),
                StartAtUtc = DateTimeOffset.UtcNow.AddDays(1),
                EndAtUtc = DateTimeOffset.UtcNow.AddDays(2),
                Status = ReservationStatus.Confirmed,
                CreatedAtUtc = DateTimeOffset.UtcNow
            });

        var result = await _sut.GetByIdAsync(id, "other-user", isAdmin: false);

        Assert.NotNull(result);
        Assert.Equal("owner", result!.UserId);
    }

    [Fact]
    public async Task GetByIdAsync_AsAdmin_ReturnsOtherUsersReservation()
    {
        var id = Guid.NewGuid();
        _reservations.Setup(r => r.GetByIdAsync(id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new Reservation
            {
                Id = id,
                UserId = "owner",
                IgloUnitId = Guid.NewGuid(),
                StartAtUtc = DateTimeOffset.UtcNow.AddDays(1),
                EndAtUtc = DateTimeOffset.UtcNow.AddDays(2),
                Status = ReservationStatus.Confirmed,
                CreatedAtUtc = DateTimeOffset.UtcNow
            });

        var result = await _sut.GetByIdAsync(id, "admin", isAdmin: true);

        Assert.NotNull(result);
        Assert.Equal("owner", result!.UserId);
    }

    [Fact]
    public async Task SoftDeleteAsync_WritesAuditLog()
    {
        var id = Guid.NewGuid();
        _reservations.Setup(r => r.GetByIdAsync(id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new Reservation
            {
                Id = id,
                UserId = "owner",
                IgloUnitId = Guid.NewGuid(),
                StartAtUtc = DateTimeOffset.UtcNow.AddDays(1),
                EndAtUtc = DateTimeOffset.UtcNow.AddDays(2)
            });

        var ok = await _sut.SoftDeleteAsync(id, "admin-1");

        Assert.True(ok);
        _audit.Verify(a => a.AddAsync(
            It.Is<AuditLog>(l => l.Action == "SoftDelete" && l.EntityId == id.ToString() && l.ActorUserId == "admin-1"),
            It.IsAny<CancellationToken>()), Times.Once);
    }
}
