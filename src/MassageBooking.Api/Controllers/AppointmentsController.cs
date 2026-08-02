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
        if (!result.IsSuccess) return BadRequest(result.Error);
        return Ok(result.Value);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(Guid id)
    {
        var result = await _appointmentService.GetByIdAsync(id);
        if (!result.IsSuccess) return NotFound(result.Error);
        return Ok(result.Value);
    }

    [HttpPost]
    public async Task<IActionResult> Book([FromBody] BookAppointmentRequest request)
    {
        var clientId = GetUserId();
        if (clientId == null) return Unauthorized();

        var result = await _appointmentService.BookAsync(request, clientId.Value);
        if (!result.IsSuccess) return BadRequest(result.Error);
        return CreatedAtAction(nameof(GetById), new { id = result.Value!.Id }, result.Value);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateAppointmentRequest request)
    {
        var result = await _appointmentService.UpdateAsync(id, request);
        if (!result.IsSuccess) return BadRequest(result.Error);
        return Ok(result.Value);
    }

    [HttpDelete("{id}")]
    [Authorize(Policy = "TherapistOnly")]
    public async Task<IActionResult> Delete(Guid id, [FromQuery] string? reason = null)
    {
        var result = await _appointmentService.CancelAsync(id, reason ?? "Deleted by therapist");
        if (!result.IsSuccess) return BadRequest(result.Error);
        return Ok(result.Value);
    }

    [HttpPost("{id}/confirm")]
    public async Task<IActionResult> Confirm(Guid id)
    {
        var result = await _appointmentService.ConfirmAsync(id);
        if (!result.IsSuccess) return BadRequest(result.Error);
        return Ok(result.Value);
    }

    [HttpPost("{id}/cancel")]
    public async Task<IActionResult> Cancel(Guid id, [FromBody] CancelRequest? request = null)
    {
        var result = await _appointmentService.CancelAsync(id, request?.Reason);
        if (!result.IsSuccess) return BadRequest(result.Error);
        return Ok(result.Value);
    }

    [HttpGet("slots")]
    public async Task<IActionResult> GetSlots(
        [FromQuery] Guid therapistId,
        [FromQuery] Guid serviceTypeId,
        [FromQuery] DateTime date)
    {
        var result = await _appointmentService.GetAvailableSlotsAsync(therapistId, serviceTypeId, date);
        if (!result.IsSuccess) return BadRequest(result.Error);
        return Ok(result.Value);
    }

    [HttpGet("upcoming")]
    public async Task<IActionResult> GetUpcoming(
        [FromQuery] Guid? therapistId = null,
        [FromQuery] int limit = 10)
    {
        var result = await _appointmentService.GetUpcomingAsync(therapistId, limit);
        if (!result.IsSuccess) return BadRequest(result.Error);
        return Ok(result.Value);
    }

    [HttpGet("client/{clientId}")]
    public async Task<IActionResult> GetByClient(Guid clientId)
    {
        var result = await _appointmentService.GetByClientAsync(clientId);
        if (!result.IsSuccess) return BadRequest(result.Error);
        return Ok(result.Value);
    }

    [HttpGet("therapist/{therapistId}")]
    [Authorize(Policy = "TherapistOnly")]
    public async Task<IActionResult> GetByTherapist(Guid therapistId)
    {
        var result = await _appointmentService.GetByTherapistAsync(therapistId);
        if (!result.IsSuccess) return BadRequest(result.Error);
        return Ok(result.Value);
    }

    private Guid? GetUserId()
    {
        var claim = User.FindFirst(ClaimTypes.NameIdentifier);
        return claim != null ? Guid.Parse(claim.Value) : null;
    }
}

public record CancelRequest(string? Reason);
