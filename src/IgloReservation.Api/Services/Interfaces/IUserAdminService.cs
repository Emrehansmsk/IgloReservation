using IgloReservation.Api.DTOs.Common;
using IgloReservation.Api.DTOs.Users;

namespace IgloReservation.Api.Services.Interfaces;

public interface IUserAdminService
{
    Task<PagedResult<UserListItemResponse>> ListAsync(int page, int pageSize, CancellationToken cancellationToken = default);

    Task<UserListItemResponse?> GetByIdAsync(string id, CancellationToken cancellationToken = default);

    Task<bool> SetRoleAsync(string id, string role, CancellationToken cancellationToken = default);
}
