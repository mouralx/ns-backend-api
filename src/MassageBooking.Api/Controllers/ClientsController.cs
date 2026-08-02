using MassageBooking.Application.DTOs;
using MassageBooking.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace MassageBooking.Api.Controllers;

[ApiController]
[Route("api/clients")]
[Authorize]
public class ClientsController : ControllerBase
{
    private readonly IClientService _clientService;

    public ClientsController(IClientService clientService)
    {
        _clientService = clientService;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var result = await _clientService.GetAllAsync();
        if (!result.IsSuccess) return BadRequest(result.Error);
        return Ok(new ApiResponse<IEnumerable<ClientDto>>(result.Value));
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(Guid id)
    {
        var result = await _clientService.GetByIdAsync(id);
        if (!result.IsSuccess) return NotFound(result.Error);
        return Ok(new ApiResponse<ClientDto>(result.Value));
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateClientRequest request)
    {
        var result = await _clientService.UpdateAsync(id, request);
        if (!result.IsSuccess) return BadRequest(result.Error);
        return Ok(new ApiResponse<ClientDto>(result.Value));
    }

    [HttpGet("{id}/appointments")]
    public async Task<IActionResult> GetAppointmentHistory(Guid id)
    {
        var result = await _clientService.GetAppointmentHistoryAsync(id);
        if (!result.IsSuccess) return BadRequest(result.Error);
        return Ok(new ApiResponse<IEnumerable<AppointmentDto>>(result.Value));
    }
}
