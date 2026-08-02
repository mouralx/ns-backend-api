namespace MassageBooking.Application.DTOs;

public record ClientDto(
    Guid Id,
    string Email,
    string Phone,
    string Name,
    bool IsActive,
    DateTime CreatedAt
);

public record UpdateClientRequest(
    string? Phone,
    string? Name,
    bool? IsActive
);
