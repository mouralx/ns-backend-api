using System.Security.Claims;
using MassageBooking.Application.DTOs;
using MassageBooking.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace MassageBooking.Api.Controllers;

[ApiController]
[Route("api/availability")]
[Authorize]
public class AvailabilityController : ControllerBase
{
    private readonly IAvailabilityService _availabilityService;
    private readonly IAuthService _authService;

    public AvailabilityController(IAvailabilityService availabilityService, IAuthService authService)
    {
        _availabilityService = availabilityService;
        _authService = authService;
    }

    [HttpGet("rules")]
    public async Task<IActionResult> GetRules([FromQuery] Guid? therapistId = null)
    {
        var id = therapistId ?? GetUserId();
        if (id == null) return BadRequest(new { error = "therapistId is required" });

        var result = await _availabilityService.GetRulesAsync(id.Value);
        if (!result.IsSuccess) return BadRequest(result.Error);
        return Ok(new ApiResponse<IEnumerable<AvailabilityRuleDto>>(result.Value));
    }

    [HttpPost("rules")]
    [Authorize(Policy = "TherapistOnly")]
    public async Task<IActionResult> CreateRule([FromQuery] Guid? therapistId = null, [FromBody] CreateRuleRequest request = null!)
    {
        var id = therapistId ?? GetUserId();
        if (id == null) return BadRequest(new { error = "therapistId is required" });

        var user = await _authService.GetByIdAsync(id.Value);
        if (!user.IsSuccess)
        {
            return Unauthorized(new { error = "Session is no longer valid. Please sign in again." });
        }

        var result = await _availabilityService.CreateRuleAsync(id.Value, request);
        if (!result.IsSuccess) return BadRequest(new { error = result.Error });
        return CreatedAtAction(nameof(GetRules), new { therapistId = id }, new ApiResponse<AvailabilityRuleDto>(result.Value));
    }

    [HttpPut("rules/{ruleId}")]
    [Authorize(Policy = "TherapistOnly")]
    public async Task<IActionResult> UpdateRule(Guid ruleId, [FromQuery] Guid? therapistId = null, [FromBody] UpdateRuleRequest request = null!)
    {
        var id = therapistId ?? GetUserId();
        if (id == null) return BadRequest(new { error = "therapistId is required" });

        var result = await _availabilityService.UpdateRuleAsync(id.Value, ruleId, request);
        if (!result.IsSuccess) return BadRequest(result.Error);
        return Ok(new ApiResponse<AvailabilityRuleDto>(result.Value));
    }

    [HttpDelete("rules/{ruleId}")]
    [Authorize(Policy = "TherapistOnly")]
    public async Task<IActionResult> DeleteRule(Guid ruleId)
    {
        var therapistId = GetUserId();
        if (therapistId == null) return Unauthorized();

        var result = await _availabilityService.DeleteRuleAsync(therapistId.Value, ruleId);
        if (!result.IsSuccess) return NotFound(new { error = result.Error });
        return NoContent();
    }

    [HttpGet("blocks")]
    public async Task<IActionResult> GetBlocks([FromQuery] Guid? therapistId = null)
    {
        var id = therapistId ?? GetUserId();
        if (id == null) return BadRequest(new { error = "therapistId is required" });

        var result = await _availabilityService.GetBlocksAsync(id.Value);
        if (!result.IsSuccess) return BadRequest(result.Error);
        return Ok(new ApiResponse<IEnumerable<AvailabilityBlockDto>>(result.Value));
    }

    [HttpPost("blocks")]
    [Authorize(Policy = "TherapistOnly")]
    public async Task<IActionResult> CreateBlock([FromQuery] Guid? therapistId = null, [FromBody] CreateBlockRequest request = null!)
    {
        var id = therapistId ?? GetUserId();
        if (id == null) return BadRequest(new { error = "therapistId is required" });

        var result = await _availabilityService.CreateBlockAsync(id.Value, request);
        if (!result.IsSuccess) return BadRequest(result.Error);
        return CreatedAtAction(nameof(GetBlocks), new { therapistId = id }, new ApiResponse<AvailabilityBlockDto>(result.Value));
    }

    [HttpPut("blocks/{blockId}")]
    [Authorize(Policy = "TherapistOnly")]
    public async Task<IActionResult> UpdateBlock(Guid blockId, [FromQuery] Guid? therapistId = null, [FromBody] UpdateBlockRequest request = null!)
    {
        var id = therapistId ?? GetUserId();
        if (id == null) return BadRequest(new { error = "therapistId is required" });

        var result = await _availabilityService.UpdateBlockAsync(id.Value, blockId, request);
        if (!result.IsSuccess) return BadRequest(result.Error);
        return Ok(new ApiResponse<AvailabilityBlockDto>(result.Value));
    }

    [HttpDelete("blocks/{blockId}")]
    [Authorize(Policy = "TherapistOnly")]
    public async Task<IActionResult> DeleteBlock(Guid blockId)
    {
        var result = await _availabilityService.DeleteBlockAsync(blockId);
        if (!result.IsSuccess) return BadRequest(result.Error);
        return NoContent();
    }

    private Guid? GetUserId()
    {
        var claim = User.FindFirst(ClaimTypes.NameIdentifier);
        return claim != null ? Guid.Parse(claim.Value) : null;
    }
}
