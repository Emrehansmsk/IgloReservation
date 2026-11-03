using System.ComponentModel.DataAnnotations;
using IgloReservation.Api.DTOs.Auth;
using IgloReservation.Api.DTOs.IgloUnits;
using IgloReservation.Api.DTOs.Reservations;
using IgloReservation.Api.Models.Entities;

namespace IgloReservation.Api.Tests;

public class DtoValidationTests
{
    private static IList<ValidationResult> Validate(object model)
    {
        var results = new List<ValidationResult>();
        Validator.TryValidateObject(model, new ValidationContext(model), results, validateAllProperties: true);
        return results;
    }

    [Fact]
    public void LoginRequest_RequiresEmailAndPassword()
    {
        var results = Validate(new LoginRequest());
        Assert.Contains(results, r => r.MemberNames.Contains(nameof(LoginRequest.Email)));
        Assert.Contains(results, r => r.MemberNames.Contains(nameof(LoginRequest.Password)));
    }

    [Fact]
    public void RegisterRequest_RejectsShortPassword()
    {
        var results = Validate(new RegisterRequest
        {
            Email = "user@example.com",
            Password = "short"
        });
        Assert.Contains(results, r => r.MemberNames.Contains(nameof(RegisterRequest.Password)));
    }

    [Fact]
    public void CreateReservationRequest_RejectsEndBeforeStart()
    {
        var start = DateTimeOffset.UtcNow.AddDays(1);
        var results = Validate(new CreateReservationRequest
        {
            UserId = "u1",
            IgloUnitId = Guid.NewGuid(),
            StartAtUtc = start,
            EndAtUtc = start,
            Status = ReservationStatus.Pending
        });
        Assert.Contains(results, r => r.ErrorMessage!.Contains("EndAtUtc"));
    }

    [Fact]
    public void CreateIgloUnitRequest_RejectsEmptyName()
    {
        var results = Validate(new CreateIgloUnitRequest { Name = "" });
        Assert.NotEmpty(results);
    }

    [Fact]
    public void CreateReservationRequest_ValidWhenRangeOk()
    {
        var results = Validate(new CreateReservationRequest
        {
            UserId = "u1",
            IgloUnitId = Guid.NewGuid(),
            StartAtUtc = DateTimeOffset.UtcNow.AddDays(1),
            EndAtUtc = DateTimeOffset.UtcNow.AddDays(2),
            Status = ReservationStatus.Pending
        });
        Assert.Empty(results);
    }
}
