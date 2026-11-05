using DndJornada.Application.DTOs;
using DndJornada.Application.Services;
using DndJornada.Domain.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DndJornada.Api.Controllers;

[Authorize]
public class AdjustmentsController : BaseApiController
{
    private readonly IAdjustmentService _adjustmentService;

    public AdjustmentsController(IAdjustmentService adjustmentService)
    {
        _adjustmentService = adjustmentService;
    }

    [HttpPost]
    public async Task<ActionResult<AdjustmentResponse>> Create([FromBody] CreateAdjustmentRequest request, CancellationToken ct)
    {
        try
        {
            var result = await _adjustmentService.CreateRequestAsync(CurrentUserId, request, ct);
            return CreatedAtAction(nameof(GetMyAdjustments), new { id = result.Id }, result);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpGet("my")]
    public async Task<ActionResult<IReadOnlyList<AdjustmentResponse>>> GetMyAdjustments(CancellationToken ct)
    {
        var list = await _adjustmentService.GetMyAdjustmentsAsync(CurrentUserId, ct);
        return Ok(list);
    }

    [HttpGet("pending")]
    [Authorize(Roles = "Manager,HR")]
    public async Task<ActionResult<IReadOnlyList<AdjustmentResponse>>> GetPendingAdjustments(CancellationToken ct)
    {
        if (CurrentUserRole == UserRole.HR)
        {
            var allPending = await _adjustmentService.GetAllPendingAsync(ct);
            return Ok(allPending);
        }

        var teamPending = await _adjustmentService.GetPendingForManagerAsync(CurrentUserId, ct);
        return Ok(teamPending);
    }

    [HttpPut("{id:guid}/review")]
    [Authorize(Roles = "Manager,HR")]
    public async Task<ActionResult<AdjustmentResponse>> Review(
        [FromRoute] Guid id,
        [FromBody] ReviewAdjustmentRequest request,
        CancellationToken ct)
    {
        try
        {
            var result = await _adjustmentService.ReviewAsync(id, CurrentUserId, request, ct);
            return Ok(result);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
    }
}

