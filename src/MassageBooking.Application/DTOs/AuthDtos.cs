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

public record LoginResponse(
    string AccessToken,
    string RefreshToken,
    UserDto User);

public record RefreshRequest(
    string RefreshToken);
