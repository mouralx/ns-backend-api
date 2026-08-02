using MassageBooking.Application.DTOs;
using MassageBooking.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace MassageBooking.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Policy = "TherapistOnly")]
public class ClientsController : ControllerBase
{
    private readonly IClientService _clientService;

    public ClientsController(IClientService clientService)
    {
        _clientService = clientService;
    }

    [HttpGet]
    public async Task<IActionResult> List()
    {
        var result = await _clientService.ListAsync();
        if (!result.IsSuccess)
        {
            return BadRequest(new { error = result.Error });
        }

        return Ok(result.Value);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetDetail(Guid id)
    {
        var result = await _clientService.GetDetailAsync(id);
        if (!result.IsSuccess)
        {
            return NotFound(new { error = result.Error });
        }

        return Ok(result.Value);
    }

    [HttpGet("{id}/appointments")]
    public async Task<IActionResult> GetAppointments(Guid id)
    {
        var result = await _clientService.GetAppointmentsAsync(id);
        if (!result.IsSuccess)
        {
            return BadRequest(new { error = result.Error });
        }

        return Ok(result.Value);
    }
}
