namespace MassageBooking.Application.DTOs;

public record ServiceTypeDto(
    Guid Id,
    string Name,
    int DurationMin,
    string Description,
    bool IsActive);

public record CreateServiceTypeRequest(
    string Name,
    int DurationMin,
    string Description);

public record UpdateServiceTypeRequest(
    string? Name,
    int? DurationMin,
    string? Description,
    bool? IsActive);
