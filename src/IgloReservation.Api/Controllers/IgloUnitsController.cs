using IgloReservation.Api.Configurations;
using IgloReservation.Api.DTOs.Common;
using IgloReservation.Api.DTOs.IgloUnits;
using IgloReservation.Api.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace IgloReservation.Api.Controllers;

/// <summary>
/// Iglo unit resources. Reads: Admin and User. Writes: Admin only.
/// </summary>
[ApiController]
[Route("api/iglo-units")]
[Authorize]
[EnableRateLimiting("api")]
[Produces("application/json")]
public class IgloUnitsController : ControllerBase
{
    private readonly IIgloUnitService _units;

    public IgloUnitsController(IIgloUnitService units)
    {
        _units = units;
    }

    /// <summary>
    /// Lists iglo units (paginated).
    /// </summary>
    /// <remarks>Roles: <b>Admin</b>, <b>User</b>.</remarks>
    /// <response code="200">Paged unit list.</response>
    /// <response code="401">Missing or invalid token.</response>
    /// <response code="403">Role not allowed.</response>
    [HttpGet]
    [Authorize(Roles = $"{AppRoles.Admin},{AppRoles.User}")]
    [ProducesResponseType(typeof(PagedResult<IgloUnitResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<PagedResult<IgloUnitResponse>>> List(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken cancellationToken = default)
    {
        return Ok(await _units.ListAsync(page, pageSize, cancellationToken));
    }

    /// <summary>
    /// Gets an iglo unit by id.
    /// </summary>
    /// <remarks>Roles: <b>Admin</b>, <b>User</b>.</remarks>
    /// <response code="200">Unit found.</response>
    /// <response code="401">Missing or invalid token.</response>
    /// <response code="403">Role not allowed.</response>
    /// <response code="404">Unit not found.</response>
    [HttpGet("{id:guid}")]
    [Authorize(Roles = $"{AppRoles.Admin},{AppRoles.User}")]
    [ProducesResponseType(typeof(IgloUnitResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<IgloUnitResponse>> GetById(Guid id, CancellationToken cancellationToken)
    {
        var item = await _units.GetByIdAsync(id, cancellationToken);
        return item is null ? NotFound() : Ok(item);
    }

    /// <summary>
    /// Creates an iglo unit.
    /// </summary>
    /// <remarks><b>Admin only.</b></remarks>
    /// <response code="201">Created.</response>
    /// <response code="400">Validation failed.</response>
    /// <response code="401">Missing or invalid token.</response>
    /// <response code="403">Caller is not Admin.</response>
    [HttpPost]
    [Authorize(Roles = AppRoles.Admin)]
    [ProducesResponseType(typeof(IgloUnitResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<IgloUnitResponse>> Create(
        [FromBody] CreateIgloUnitRequest request,
        CancellationToken cancellationToken)
    {
        var created = await _units.CreateAsync(request, cancellationToken);
        return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
    }

    /// <summary>
    /// Updates an iglo unit.
    /// </summary>
    /// <remarks><b>Admin only.</b></remarks>
    /// <response code="200">Updated.</response>
    /// <response code="400">Validation failed.</response>
    /// <response code="401">Missing or invalid token.</response>
    /// <response code="403">Caller is not Admin.</response>
    /// <response code="404">Unit not found.</response>
    [HttpPut("{id:guid}")]
    [Authorize(Roles = AppRoles.Admin)]
    [ProducesResponseType(typeof(IgloUnitResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<IgloUnitResponse>> Update(
        Guid id,
        [FromBody] UpdateIgloUnitRequest request,
        CancellationToken cancellationToken)
    {
        var updated = await _units.UpdateAsync(id, request, cancellationToken);
        return updated is null ? NotFound() : Ok(updated);
    }

    /// <summary>
    /// Soft-deletes an iglo unit.
    /// </summary>
    /// <remarks><b>Admin only.</b></remarks>
    /// <response code="204">Deleted.</response>
    /// <response code="401">Missing or invalid token.</response>
    /// <response code="403">Caller is not Admin.</response>
    /// <response code="404">Unit not found.</response>
    [HttpDelete("{id:guid}")]
    [Authorize(Roles = AppRoles.Admin)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
    {
        var deleted = await _units.SoftDeleteAsync(id, cancellationToken);
        return deleted ? NoContent() : NotFound();
    }
}
