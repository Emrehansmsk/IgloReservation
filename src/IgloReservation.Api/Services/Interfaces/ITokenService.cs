using IgloReservation.Api.Models.Entities;

namespace IgloReservation.Api.Services.Interfaces;

public interface ITokenService
{
    (string AccessToken, DateTimeOffset ExpiresAtUtc) CreateAccessToken(
        ApplicationUser user,
        IEnumerable<string> roles);
}
