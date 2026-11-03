using IgloReservation.Api.Configurations;
using IgloReservation.Api.DTOs.Common;
using IgloReservation.Api.DTOs.Users;
using IgloReservation.Api.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace IgloReservation.Api.Controllers;

/// <summary>
/// User administration. <b>Admin only</b> for all actions. Passwords are never returned.
/// </summary>
[ApiController]
[Route("api/users")]
[Authorize(Roles = AppRoles.Admin)]
[EnableRateLimiting("api")]
[Produces("application/json")]
public class UsersController : ControllerBase
{
    private readonly IUserAdminService _users;

    public UsersController(IUserAdminService users)
    {
        _users = users;
    }

    /// <summary>
    /// Lists users (paginated).
    /// </summary>
    /// <remarks><b>Admin only.</b></remarks>
    /// <response code="200">Paged user list.</response>
    /// <response code="401">Missing or invalid token.</response>
    /// <response code="403">Caller is not Admin.</response>
    [HttpGet]
    [ProducesResponseType(typeof(PagedResult<UserListItemResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<PagedResult<UserListItemResponse>>> List(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken cancellationToken = default)
    {
        return Ok(await _users.ListAsync(page, pageSize, cancellationToken));
    }

    /// <summary>
    /// Gets a user by id.
    /// </summary>
    /// <remarks><b>Admin only.</b></remarks>
    /// <response code="200">User found.</response>
    /// <response code="401">Missing or invalid token.</response>
    /// <response code="403">Caller is not Admin.</response>
    /// <response code="404">User not found.</response>
    [HttpGet("{id}")]
    [ProducesResponseType(typeof(UserListItemResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<UserListItemResponse>> GetById(string id, CancellationToken cancellationToken)
    {
        var user = await _users.GetByIdAsync(id, cancellationToken);
        return user is null ? NotFound() : Ok(user);
    }

    /// <summary>
    /// Sets the user's role (<c>Admin</c> or <c>User</c>).
    /// </summary>
    /// <remarks><b>Admin only.</b></remarks>
    /// <response code="204">Role updated.</response>
    /// <response code="400">Invalid role value.</response>
    /// <response code="401">Missing or invalid token.</response>
    /// <response code="403">Caller is not Admin.</response>
    /// <response code="404">User not found.</response>
    [HttpPatch("{id}/role")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> SetRole(
        string id,
        [FromBody] SetRoleRequest request,
        CancellationToken cancellationToken)
    {
        var ok = await _users.SetRoleAsync(id, request.Role, cancellationToken);
        return ok ? NoContent() : NotFound();
    }
}
