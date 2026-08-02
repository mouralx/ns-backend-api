using System.Security.Claims;
using MassageBooking.Application.DTOs;
using MassageBooking.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace MassageBooking.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Policy = "TherapistOnly")]
public class AvailabilityController : ControllerBase
{
    private readonly IAvailabilityService _availabilityService;

    public AvailabilityController(IAvailabilityService availabilityService)
    {
        _availabilityService = availabilityService;
    }

    [HttpGet("rules/{therapistId}")]
    public async Task<IActionResult> GetRules(Guid therapistId)
    {
        var result = await _availabilityService.GetRulesAsync(therapistId);
        if (!result.IsSuccess)
        {
            return BadRequest(new { error = result.Error });
        }

        return Ok(result.Value);
    }

    [HttpPost("rules/{therapistId}")]
    public async Task<IActionResult> CreateRule(Guid therapistId, [FromBody] CreateRuleRequest request)
    {
        var result = await _availabilityService.CreateRuleAsync(therapistId, request);
        if (!result.IsSuccess)
        {
            return BadRequest(new { error = result.Error });
        }

        return Created("", result.Value);
    }

    [HttpDelete("rules/{ruleId}")]
    public async Task<IActionResult> DeleteRule(Guid ruleId)
    {
        var result = await _availabilityService.DeleteRuleAsync(ruleId);
        if (!result.IsSuccess)
        {
            return BadRequest(new { error = result.Error });
        }

        return NoContent();
    }

    [HttpGet("blocks/{therapistId}")]
    public async Task<IActionResult> GetBlocks(Guid therapistId)
    {
        var result = await _availabilityService.GetBlocksAsync(therapistId);
        if (!result.IsSuccess)
        {
            return BadRequest(new { error = result.Error });
        }

        return Ok(result.Value);
    }

    [HttpPost("blocks/{therapistId}")]
    public async Task<IActionResult> CreateBlock(Guid therapistId, [FromBody] CreateBlockRequest request)
    {
        var result = await _availabilityService.CreateBlockAsync(therapistId, request);
        if (!result.IsSuccess)
        {
            return BadRequest(new { error = result.Error });
        }

        return Created("", result.Value);
    }

    [HttpDelete("blocks/{blockId}")]
    public async Task<IActionResult> DeleteBlock(Guid blockId)
    {
        var result = await _availabilityService.DeleteBlockAsync(blockId);
        if (!result.IsSuccess)
        {
            return BadRequest(new { error = result.Error });
        }

        return NoContent();
    }
}
