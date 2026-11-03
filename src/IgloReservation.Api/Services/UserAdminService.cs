using IgloReservation.Api.Configurations;
using IgloReservation.Api.DTOs.Common;
using IgloReservation.Api.DTOs.Users;
using IgloReservation.Api.Models.Entities;
using IgloReservation.Api.Services.Interfaces;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace IgloReservation.Api.Services;

public class UserAdminService : IUserAdminService
{
    private readonly UserManager<ApplicationUser> _userManager;

    public UserAdminService(UserManager<ApplicationUser> userManager)
    {
        _userManager = userManager;
    }

    public async Task<PagedResult<UserListItemResponse>> ListAsync(
        int page,
        int pageSize,
        CancellationToken cancellationToken = default)
    {
        page = page < 1 ? 1 : page;
        pageSize = pageSize < 1 ? 20 : Math.Min(pageSize, 100);

        var query = _userManager.Users.AsNoTracking().OrderBy(u => u.Email);
        var total = await query.CountAsync(cancellationToken);
        var users = await query.Skip((page - 1) * pageSize).Take(pageSize).ToListAsync(cancellationToken);

        var items = new List<UserListItemResponse>();
        foreach (var user in users)
        {
            var roles = await _userManager.GetRolesAsync(user);
            items.Add(new UserListItemResponse
            {
                Id = user.Id,
                Email = user.Email,
                UserName = user.UserName,
                Roles = roles.ToList()
            });
        }

        return new PagedResult<UserListItemResponse>
        {
            Items = items,
            Page = page,
            PageSize = pageSize,
            TotalCount = total
        };
    }

    public async Task<UserListItemResponse?> GetByIdAsync(string id, CancellationToken cancellationToken = default)
    {
        var user = await _userManager.FindByIdAsync(id);
        if (user is null)
        {
            return null;
        }

        var roles = await _userManager.GetRolesAsync(user);
        return new UserListItemResponse
        {
            Id = user.Id,
            Email = user.Email,
            UserName = user.UserName,
            Roles = roles.ToList()
        };
    }

    public async Task<bool> SetRoleAsync(string id, string role, CancellationToken cancellationToken = default)
    {
        if (!AppRoles.All.Contains(role, StringComparer.Ordinal))
        {
            throw new ArgumentException($"Role must be one of: {string.Join(", ", AppRoles.All)}");
        }

        var user = await _userManager.FindByIdAsync(id);
        if (user is null)
        {
            return false;
        }

        var current = await _userManager.GetRolesAsync(user);
        if (current.Count > 0)
        {
            await _userManager.RemoveFromRolesAsync(user, current);
        }

        var result = await _userManager.AddToRoleAsync(user, role);
        if (!result.Succeeded)
        {
            throw new InvalidOperationException(string.Join("; ", result.Errors.Select(e => e.Description)));
        }

        return true;
    }
}
