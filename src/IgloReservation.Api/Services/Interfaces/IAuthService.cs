using IgloReservation.Api.DTOs.Auth;

namespace IgloReservation.Api.Services.Interfaces;

public interface IAuthService
{
    Task<AuthResponse> LoginAsync(LoginRequest request, CancellationToken cancellationToken = default);

    /// <summary>
    /// Always assigns the User role. Admin via register is rejected.
    /// </summary>
    Task<AuthResponse> RegisterAsync(RegisterRequest request, CancellationToken cancellationToken = default);

    Task<UserResponse?> GetCurrentUserAsync(string userId, CancellationToken cancellationToken = default);
}
