using IgloReservation.Api.DTOs.Reservations;
using IgloReservation.Api.Exceptions;
using IgloReservation.Api.Models.Entities;
using IgloReservation.Api.Repositories.Interfaces;
using IgloReservation.Api.Services;
using Moq;

namespace IgloReservation.Api.Tests;

public class ReservationServiceScheduleTests
{
    private readonly Mock<IReservationRepository> _reservations = new();
    private readonly Mock<IAuditLogRepository> _audit = new();
    private readonly ReservationService _sut;

    public ReservationServiceScheduleTests()
    {
        _sut = new ReservationService(_reservations.Object, _audit.Object, TestMapper.Create());
    }

    [Fact]
    public void ValidateSchedule_RejectsEndBeforeOrEqualStart()
    {
        var start = DateTimeOffset.UtcNow.AddDays(1);
        Assert.Throws<BusinessRuleException>(() =>
            ReservationService.ValidateSchedule(start, start, enforceFuturePolicy: true));
    }

    [Fact]
    public void ValidateSchedule_RejectsFullyPastRange()
    {
        var start = DateTimeOffset.UtcNow.AddDays(-2);
        var end = DateTimeOffset.UtcNow.AddDays(-1);
        Assert.Throws<BusinessRuleException>(() =>
            ReservationService.ValidateSchedule(start, end, enforceFuturePolicy: true));
    }

    [Fact]
    public void ValidateSchedule_AllowsNearPastStartWithinGrace()
    {
        var start = DateTimeOffset.UtcNow.AddMinutes(-5);
        var end = DateTimeOffset.UtcNow.AddHours(2);
        ReservationService.ValidateSchedule(start, end, enforceFuturePolicy: true);
    }

    [Fact]
    public async Task CreateAsync_ThrowsConflict_WhenOverlapExists()
    {
        var unitId = Guid.NewGuid();
        _reservations
            .Setup(r => r.HasOverlappingActiveAsync(
                unitId,
                It.IsAny<DateTimeOffset>(),
                It.IsAny<DateTimeOffset>(),
                null,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        var request = new CreateReservationRequest
        {
            UserId = "u1",
            IgloUnitId = unitId,
            StartAtUtc = DateTimeOffset.UtcNow.AddDays(1),
            EndAtUtc = DateTimeOffset.UtcNow.AddDays(2),
            Status = ReservationStatus.Pending
        };

        await Assert.ThrowsAsync<ReservationConflictException>(() => _sut.CreateAsync(request));
    }

    [Fact]
    public void ReservationStatus_PendingAndConfirmed_AreActiveForConflict()
    {
        Assert.True(ReservationStatus.Pending.IsActiveForConflict());
        Assert.True(ReservationStatus.Confirmed.IsActiveForConflict());
        Assert.False(ReservationStatus.Cancelled.IsActiveForConflict());
        Assert.False(ReservationStatus.Completed.IsActiveForConflict());
    }
}
