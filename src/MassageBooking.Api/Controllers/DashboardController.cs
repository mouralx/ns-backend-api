using System.Security.Claims;
using MassageBooking.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace MassageBooking.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Policy = "TherapistOnly")]
public class DashboardController : ControllerBase
{
    private readonly IDashboardService _dashboardService;

    public DashboardController(IDashboardService dashboardService)
    {
        _dashboardService = dashboardService;
    }

    [HttpGet("today")]
    public async Task<IActionResult> GetTodaySchedule()
    {
        var therapistId = GetUserId();
        if (therapistId == null)
        {
            return Unauthorized();
        }

        var result = await _dashboardService.GetTodayScheduleAsync(therapistId.Value);
        if (!result.IsSuccess)
        {
            return BadRequest(new { error = result.Error });
        }

        return Ok(result.Value);
    }

    [HttpGet("stats")]
    public async Task<IActionResult> GetStats()
    {
        var therapistId = GetUserId();
        if (therapistId == null)
        {
            return Unauthorized();
        }

        var result = await _dashboardService.GetStatsAsync(therapistId.Value);
        if (!result.IsSuccess)
        {
            return BadRequest(new { error = result.Error });
        }

        return Ok(result.Value);
    }

    [HttpGet("at-risk")]
    public async Task<IActionResult> GetAtRiskAppointments()
    {
        var therapistId = GetUserId();
        if (therapistId == null)
        {
            return Unauthorized();
        }

        var result = await _dashboardService.GetAtRiskAppointmentsAsync(therapistId.Value);
        if (!result.IsSuccess)
        {
            return BadRequest(new { error = result.Error });
        }

        return Ok(result.Value);
    }

    private Guid? GetUserId()
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
        if (userIdClaim != null && Guid.TryParse(userIdClaim.Value, out var userId))
        {
            return userId;
        }
        return null;
    }
}
