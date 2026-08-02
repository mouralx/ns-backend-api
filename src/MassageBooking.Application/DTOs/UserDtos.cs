using MassageBooking.Domain.Enums;

namespace MassageBooking.Application.DTOs;

/// <summary>
/// User DTO matching frontend expectations.
/// Fields: id, email, phone, name, role, push_token, is_active, created_at, updated_at.
/// </summary>
public record UserDto(
    Guid Id,
    string Email,
    string Phone,
    string Name,
    UserRole Role,
    string? PushToken,
    bool IsActive,
    DateTime CreatedAt,
    DateTime? UpdatedAt);

public record AuthTokens(
    string AccessToken,
    string RefreshToken,
    DateTime ExpiresAt);
