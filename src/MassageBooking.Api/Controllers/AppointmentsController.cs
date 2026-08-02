using System.Security.Claims;
using MassageBooking.Application.DTOs;
using MassageBooking.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace MassageBooking.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class AppointmentsController : ControllerBase
{
    private readonly IAppointmentService _appointmentService;

    public AppointmentsController(IAppointmentService appointmentService)
    {
        _appointmentService = appointmentService;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var result = await _appointmentService.GetAllAsync();
        if (!result.IsSuccess)
        {
            return BadRequest(new { error = result.Error });
        }

        return Ok(result.Value);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(Guid id)
    {
        var result = await _appointmentService.GetByIdAsync(id);
        if (!result.IsSuccess)
        {
            return NotFound(new { error = result.Error });
        }

        return Ok(result.Value);
    }

    [HttpGet("client/{clientId}")]
    public async Task<IActionResult> GetByClient(Guid clientId)
    {
        var result = await _appointmentService.GetByClientAsync(clientId);
        if (!result.IsSuccess)
        {
            return BadRequest(new { error = result.Error });
        }

        return Ok(result.Value);
    }

    [HttpGet("therapist/{therapistId}")]
    public async Task<IActionResult> GetByTherapist(Guid therapistId)
    {
        var result = await _appointmentService.GetByTherapistAsync(therapistId);
        if (!result.IsSuccess)
        {
            return BadRequest(new { error = result.Error });
        }

        return Ok(result.Value);
    }

    [HttpGet("slots")]
    public async Task<IActionResult> GetAvailableSlots(
        [FromQuery] Guid therapistId,
        [FromQuery] Guid serviceTypeId,
        [FromQuery] DateTime date)
    {
        var result = await _appointmentService.GetAvailableSlotsAsync(therapistId, serviceTypeId, date);
        if (!result.IsSuccess)
        {
            return BadRequest(new { error = result.Error });
        }

        return Ok(result.Value);
    }

    [HttpPost]
    public async Task<IActionResult> Book([FromBody] BookAppointmentRequest request)
    {
        var clientId = GetUserId();
        if (clientId == null)
        {
            return Unauthorized();
        }

        var result = await _appointmentService.BookAsync(request, clientId.Value);
        if (!result.IsSuccess)
        {
            return BadRequest(new { error = result.Error });
        }

        return CreatedAtAction(nameof(GetById), new { id = result.Value.Id }, result.Value);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateAppointmentRequest request)
    {
        // In a real implementation, you would have an update method
        return Ok(new { message = "Update endpoint" });
    }

    [HttpPost("{id}/confirm")]
    public async Task<IActionResult> Confirm(Guid id)
    {
        var result = await _appointmentService.ConfirmAsync(id);
        if (!result.IsSuccess)
        {
            return BadRequest(new { error = result.Error });
        }

        return Ok(result.Value);
    }

    [HttpPost("{id}/cancel")]
    public async Task<IActionResult> Cancel(Guid id, [FromQuery] string? reason = null)
    {
        var result = await _appointmentService.CancelAsync(id, reason);
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
