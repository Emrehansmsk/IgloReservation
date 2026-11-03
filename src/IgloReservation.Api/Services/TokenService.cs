using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using IgloReservation.Api.Configurations;
using IgloReservation.Api.Models.Entities;
using IgloReservation.Api.Services.Interfaces;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace IgloReservation.Api.Services;

/// <summary>
/// Issues short-lived JWT access tokens only (v1 — no refresh tokens).
/// </summary>
public class TokenService : ITokenService
{
    private readonly JwtOptions _options;
    private readonly string _signingKey;

    public TokenService(IOptions<JwtOptions> options, IConfiguration configuration, IHostEnvironment environment)
    {
        _options = options.Value;
        _signingKey = ResolveSigningKey(_options, configuration, environment);
    }

    public (string AccessToken, DateTimeOffset ExpiresAtUtc) CreateAccessToken(
        ApplicationUser user,
        IEnumerable<string> roles)
    {
        var expires = DateTimeOffset.UtcNow.AddMinutes(_options.AccessTokenMinutes <= 0 ? 60 : _options.AccessTokenMinutes);

        var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Sub, user.Id),
            new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
            new(ClaimTypes.NameIdentifier, user.Id)
        };

        if (!string.IsNullOrWhiteSpace(user.Email))
        {
            claims.Add(new Claim(JwtRegisteredClaimNames.Email, user.Email));
        }

        if (!string.IsNullOrWhiteSpace(user.UserName))
        {
            claims.Add(new Claim(ClaimTypes.Name, user.UserName));
        }

        claims.AddRange(roles.Select(r => new Claim(ClaimTypes.Role, r)));

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_signingKey));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer: _options.Issuer,
            audience: _options.Audience,
            claims: claims,
            expires: expires.UtcDateTime,
            signingCredentials: credentials);

        return (new JwtSecurityTokenHandler().WriteToken(token), expires);
    }

    private static string ResolveSigningKey(JwtOptions options, IConfiguration configuration, IHostEnvironment environment)
    {
        var key = options.SigningKey;
        if (string.IsNullOrWhiteSpace(key))
        {
            key = configuration["Jwt:SigningKey"];
        }

        if (!string.IsNullOrWhiteSpace(key))
        {
            return key;
        }

        if (environment.IsDevelopment())
        {
            return Convert.ToBase64String(Guid.NewGuid().ToByteArray())
                + Convert.ToBase64String(Guid.NewGuid().ToByteArray());
        }

        throw new InvalidOperationException(
            "Jwt:SigningKey is required. Provide it via environment variables or User Secrets — never commit it.");
    }
}
