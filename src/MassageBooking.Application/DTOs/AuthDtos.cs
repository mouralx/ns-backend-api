using MassageBooking.Domain.Enums;

namespace MassageBooking.Application.DTOs;

public record RegisterRequest(
    string Email,
    string Phone,
    string Name,
    string Password,
    UserRole? Role = null);

public record LoginRequest(
    string Email,
    string Password);

/// <summary>
/// Auth response matching frontend expectations.
/// Fields: access_token, refresh_token, token_type, expires_in, user.
/// </summary>
public record LoginResponse(
    string AccessToken,
    string RefreshToken,
    string TokenType,
    int ExpiresIn,
    UserDto User);

public record RefreshRequest(
    string RefreshToken);
