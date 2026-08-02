using MassageBooking.Domain.Enums;

namespace MassageBooking.Application.DTOs;

public record UserDto(
    Guid Id,
    string Email,
    string Phone,
    string Name,
    UserRole Role,
    bool IsActive,
    DateTime CreatedAt
);

public record AuthTokens(
    string AccessToken,
    string RefreshToken,
    DateTime ExpiresAt
);
