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

    public AvailabilityController(IAvailabilityService availabilityService)
    {
        _availabilityService = availabilityService;
    }

    [HttpGet("rules")]
    public async Task<IActionResult> GetRules([FromQuery] Guid therapistId)
    {
        var result = await _availabilityService.GetRulesAsync(therapistId);
        if (!result.IsSuccess) return BadRequest(result.Error);
        return Ok(result.Value);
    }

    [HttpPost("rules")]
    [Authorize(Policy = "TherapistOnly")]
    public async Task<IActionResult> CreateRule([FromQuery] Guid therapistId, [FromBody] CreateRuleRequest request)
    {
        var result = await _availabilityService.CreateRuleAsync(therapistId, request);
        if (!result.IsSuccess) return BadRequest(result.Error);
        return CreatedAtAction(nameof(GetRules), new { therapistId }, result.Value);
    }

    [HttpPut("rules/{ruleId}")]
    [Authorize(Policy = "TherapistOnly")]
    public async Task<IActionResult> UpdateRule(Guid ruleId, [FromQuery] Guid therapistId, [FromBody] UpdateRuleRequest request)
    {
        var result = await _availabilityService.UpdateRuleAsync(therapistId, ruleId, request);
        if (!result.IsSuccess) return BadRequest(result.Error);
        return Ok(result.Value);
    }

    [HttpDelete("rules/{ruleId}")]
    [Authorize(Policy = "TherapistOnly")]
    public async Task<IActionResult> DeleteRule(Guid ruleId)
    {
        var result = await _availabilityService.DeleteRuleAsync(ruleId);
        if (!result.IsSuccess) return BadRequest(result.Error);
        return NoContent();
    }

    [HttpGet("blocks")]
    public async Task<IActionResult> GetBlocks([FromQuery] Guid therapistId)
    {
        var result = await _availabilityService.GetBlocksAsync(therapistId);
        if (!result.IsSuccess) return BadRequest(result.Error);
        return Ok(result.Value);
    }

    [HttpPost("blocks")]
    [Authorize(Policy = "TherapistOnly")]
    public async Task<IActionResult> CreateBlock([FromQuery] Guid therapistId, [FromBody] CreateBlockRequest request)
    {
        var result = await _availabilityService.CreateBlockAsync(therapistId, request);
        if (!result.IsSuccess) return BadRequest(result.Error);
        return CreatedAtAction(nameof(GetBlocks), new { therapistId }, result.Value);
    }

    [HttpPut("blocks/{blockId}")]
    [Authorize(Policy = "TherapistOnly")]
    public async Task<IActionResult> UpdateBlock(Guid blockId, [FromQuery] Guid therapistId, [FromBody] UpdateBlockRequest request)
    {
        var result = await _availabilityService.UpdateBlockAsync(therapistId, blockId, request);
        if (!result.IsSuccess) return BadRequest(result.Error);
        return Ok(result.Value);
    }

    [HttpDelete("blocks/{blockId}")]
    [Authorize(Policy = "TherapistOnly")]
    public async Task<IActionResult> DeleteBlock(Guid blockId)
    {
        var result = await _availabilityService.DeleteBlockAsync(blockId);
        if (!result.IsSuccess) return BadRequest(result.Error);
        return NoContent();
    }
}
