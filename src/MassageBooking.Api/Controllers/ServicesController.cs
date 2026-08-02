using MassageBooking.Application.DTOs;
using MassageBooking.Domain.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace MassageBooking.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ServicesController : ControllerBase
{
    private readonly IServiceTypeRepository _serviceTypeRepository;

    public ServicesController(IServiceTypeRepository serviceTypeRepository)
    {
        _serviceTypeRepository = serviceTypeRepository;
    }

    [HttpGet]
    [AllowAnonymous]
    public async Task<IActionResult> GetAll()
    {
        var services = await _serviceTypeRepository.GetAllAsync();
        var dtos = services.Select(s => new ServiceTypeDto(
            s.Id, s.Name, s.DurationMin, s.Description, s.IsActive));

        return Ok(new ApiResponse<IEnumerable<ServiceTypeDto>>(dtos));
    }

    [HttpGet("active")]
    [AllowAnonymous]
    public async Task<IActionResult> GetActive()
    {
        var services = await _serviceTypeRepository.GetActiveAsync();
        var dtos = services.Select(s => new ServiceTypeDto(
            s.Id, s.Name, s.DurationMin, s.Description, s.IsActive));

        return Ok(new ApiResponse<IEnumerable<ServiceTypeDto>>(dtos));
    }

    [HttpGet("{id}")]
    [AllowAnonymous]
    public async Task<IActionResult> GetById(Guid id)
    {
        var service = await _serviceTypeRepository.GetByIdAsync(id);
        if (service == null)
        {
            return NotFound();
        }

        var dto = new ServiceTypeDto(
            service.Id, service.Name, service.DurationMin, service.Description, service.IsActive);

        return Ok(new ApiResponse<ServiceTypeDto>(dto));
    }

    [HttpPost]
    [Authorize(Policy = "TherapistOnly")]
    public async Task<IActionResult> Create([FromBody] CreateServiceTypeRequest request)
    {
        var service = new Domain.Entities.ServiceType
        {
            Name = request.Name,
            DurationMin = request.DurationMin,
            Description = request.Description,
            IsActive = true
        };

        var created = await _serviceTypeRepository.AddAsync(service);

        var dto = new ServiceTypeDto(
            created.Id, created.Name, created.DurationMin, created.Description, created.IsActive);

        return CreatedAtAction(nameof(GetById), new { id = dto.Id }, new ApiResponse<ServiceTypeDto>(dto));
    }

    [HttpPut("{id}")]
    [Authorize(Policy = "TherapistOnly")]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateServiceTypeRequest request)
    {
        var service = await _serviceTypeRepository.GetByIdAsync(id);
        if (service == null)
        {
            return NotFound();
        }

        if (request.Name != null) service.Name = request.Name;
        if (request.DurationMin.HasValue) service.DurationMin = request.DurationMin.Value;
        if (request.Description != null) service.Description = request.Description;
        if (request.IsActive.HasValue) service.IsActive = request.IsActive.Value;
        service.UpdatedAt = DateTime.UtcNow;

        var updated = await _serviceTypeRepository.UpdateAsync(service);

        var dto = new ServiceTypeDto(
            updated.Id, updated.Name, updated.DurationMin, updated.Description, updated.IsActive);

        return Ok(new ApiResponse<ServiceTypeDto>(dto));
    }

    [HttpDelete("{id}")]
    [Authorize(Policy = "TherapistOnly")]
    public async Task<IActionResult> Delete(Guid id)
    {
        await _serviceTypeRepository.DeleteAsync(id);
        return NoContent();
    }
}
