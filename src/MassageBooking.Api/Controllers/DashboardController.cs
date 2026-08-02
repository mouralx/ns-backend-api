using MassageBooking.Application.DTOs;
using MassageBooking.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace MassageBooking.Api.Controllers;

[ApiController]
[Route("api/dashboard")]
[Authorize]
public class DashboardController : ControllerBase
{
    private readonly IDashboardService _dashboardService;

    public DashboardController(IDashboardService dashboardService)
    {
        _dashboardService = dashboardService;
    }

    [HttpGet("stats")]
    public async Task<IActionResult> GetStats()
    {
        var result = await _dashboardService.GetStatsAsync();
        if (!result.IsSuccess) return BadRequest(result.Error);
        return Ok(new ApiResponse<DashboardStatsDto>(result.Value));
    }

    [HttpGet("today")]
    public async Task<IActionResult> GetTodaySchedule()
    {
        var result = await _dashboardService.GetTodayScheduleAsync();
        if (!result.IsSuccess) return BadRequest(result.Error);
        return Ok(result.Value);
    }

    [HttpGet("upcoming")]
    public async Task<IActionResult> GetUpcoming([FromQuery] int limit = 10)
    {
        var result = await _dashboardService.GetUpcomingAsync(limit);
        if (!result.IsSuccess) return BadRequest(result.Error);
        return Ok(result.Value);
    }

    [HttpGet("at-risk")]
    public async Task<IActionResult> GetAtRisk()
    {
        var result = await _dashboardService.GetAtRiskAppointmentsAsync();
        if (!result.IsSuccess) return BadRequest(result.Error);
        return Ok(result.Value);
    }
}
