using System.Security.Claims;
using IgloReservation.Api.Configurations;
using IgloReservation.Api.DTOs.Common;
using IgloReservation.Api.DTOs.Reservations;
using IgloReservation.Api.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace IgloReservation.Api.Controllers;

/// <summary>
/// Reservation resources. Reads: Admin and User (all). Writes: Admin only.
/// </summary>
[ApiController]
[Route("api/reservations")]
[Authorize]
[EnableRateLimiting("api")]
[Produces("application/json")]
public class ReservationsController : ControllerBase
{
    private readonly IReservationService _reservations;

    public ReservationsController(IReservationService reservations)
    {
        _reservations = reservations;
    }

    /// <summary>
    /// Lists reservations (paginated).
    /// </summary>
    /// <remarks>
    /// <para><b>Admin</b> and <b>User</b>: all reservations (board visibility).</para>
    /// <para>Create / update / delete remain <b>Admin only</b>.</para>
    /// </remarks>
    /// <response code="200">Paged reservation list.</response>
    /// <response code="401">Missing or invalid token.</response>
    /// <response code="403">Authenticated but role is not Admin or User.</response>
    [HttpGet]
    [Authorize(Roles = $"{AppRoles.Admin},{AppRoles.User}")]
    [ProducesResponseType(typeof(PagedResult<ReservationResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<PagedResult<ReservationResponse>>> List(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken cancellationToken = default)
    {
        var (userId, isAdmin) = GetCaller();
        if (userId is null)
        {
            return Unauthorized();
        }

        return Ok(await _reservations.ListAsync(userId, isAdmin, page, pageSize, cancellationToken));
    }

    /// <summary>
    /// Gets a reservation by id.
    /// </summary>
    /// <remarks>
    /// <para><b>Admin</b> and <b>User</b>: any reservation.</para>
    /// </remarks>
    /// <response code="200">Reservation found.</response>
    /// <response code="401">Missing or invalid token.</response>
    /// <response code="403">Role not allowed.</response>
    /// <response code="404">Reservation not found.</response>
    [HttpGet("{id:guid}")]
    [Authorize(Roles = $"{AppRoles.Admin},{AppRoles.User}")]
    [ProducesResponseType(typeof(ReservationResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ReservationResponse>> GetById(Guid id, CancellationToken cancellationToken)
    {
        var (userId, isAdmin) = GetCaller();
        if (userId is null)
        {
            return Unauthorized();
        }

        var item = await _reservations.GetByIdAsync(id, userId, isAdmin, cancellationToken);
        return item is null ? NotFound() : Ok(item);
    }

    /// <summary>
    /// Creates a reservation.
    /// </summary>
    /// <remarks><b>Admin only.</b> Overlapping active reservations return 409.</remarks>
    /// <response code="201">Created.</response>
    /// <response code="400">Validation or business rule failure.</response>
    /// <response code="401">Missing or invalid token.</response>
    /// <response code="403">Caller is not Admin.</response>
    /// <response code="409">Active reservation overlap.</response>
    [HttpPost]
    [Authorize(Roles = AppRoles.Admin)]
    [ProducesResponseType(typeof(ReservationResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    public async Task<ActionResult<ReservationResponse>> Create(
        [FromBody] CreateReservationRequest request,
        CancellationToken cancellationToken)
    {
        var created = await _reservations.CreateAsync(request, cancellationToken);
        return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
    }

    /// <summary>
    /// Updates a reservation.
    /// </summary>
    /// <remarks><b>Admin only.</b></remarks>
    /// <response code="200">Updated.</response>
    /// <response code="400">Validation or business rule failure.</response>
    /// <response code="401">Missing or invalid token.</response>
    /// <response code="403">Caller is not Admin.</response>
    /// <response code="404">Reservation not found.</response>
    /// <response code="409">Active reservation overlap.</response>
    [HttpPut("{id:guid}")]
    [Authorize(Roles = AppRoles.Admin)]
    [ProducesResponseType(typeof(ReservationResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    public async Task<ActionResult<ReservationResponse>> Update(
        Guid id,
        [FromBody] UpdateReservationRequest request,
        CancellationToken cancellationToken)
    {
        var updated = await _reservations.UpdateAsync(id, request, cancellationToken);
        return updated is null ? NotFound() : Ok(updated);
    }

    /// <summary>
    /// Soft-deletes a reservation.
    /// </summary>
    /// <remarks><b>Admin only.</b></remarks>
    /// <response code="204">Deleted.</response>
    /// <response code="401">Missing or invalid token.</response>
    /// <response code="403">Caller is not Admin.</response>
    /// <response code="404">Reservation not found.</response>
    [HttpDelete("{id:guid}")]
    [Authorize(Roles = AppRoles.Admin)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
    {
        var (userId, _) = GetCaller();
        var deleted = await _reservations.SoftDeleteAsync(id, userId, cancellationToken);
        return deleted ? NoContent() : NotFound();
    }

    private (string? UserId, bool IsAdmin) GetCaller()
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? User.FindFirstValue("sub");
        var isAdmin = User.IsInRole(AppRoles.Admin);
        return (userId, isAdmin);
    }
}
