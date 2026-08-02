using MassageBooking.Domain.Enums;

namespace MassageBooking.Application.DTOs;

public record UserDto(
    Guid Id,
    string Email,
    string Phone,
    string Name,
    UserRole Role,
    bool IsActive,
    DateTime CreatedAt);

public record CreateUserRequest(
    string Email,
    string Phone,
    string Name,
    string Password,
    UserRole Role);

public record UpdateUserRequest(
    string? Phone,
    string? Name,
    bool? IsActive);
