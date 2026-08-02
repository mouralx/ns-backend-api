namespace MassageBooking.Application.DTOs;

public record ClientDto(
    Guid Id,
    string Name,
    string Email,
    string Phone,
    DateTime CreatedAt);

public record ClientDetailDto(
    Guid Id,
    string Name,
    string Email,
    string Phone,
    DateTime CreatedAt,
    int TotalAppointments,
    DateTime? LastVisit);
