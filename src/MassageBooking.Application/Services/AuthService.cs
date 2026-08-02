using System.Security.Claims;
using MassageBooking.Application.DTOs;
using MassageBooking.Application.Interfaces;
using MassageBooking.Domain.Common;
using MassageBooking.Domain.Entities;
using MassageBooking.Domain.Enums;
using MassageBooking.Domain.Interfaces;

namespace MassageBooking.Application.Services;

public class AuthService : IAuthService
{
    private readonly IUserRepository _userRepository;
    private readonly IJwtService _jwtService;

    public AuthService(IUserRepository userRepository, IJwtService jwtService)
    {
        _userRepository = userRepository;
        _jwtService = jwtService;
    }

    public async Task<Result<UserDto>> RegisterAsync(RegisterRequest request)
    {
        var existingUser = await _userRepository.GetByEmailAsync(request.Email);
        if (existingUser != null)
        {
            return Result<UserDto>.Failure("Email already registered");
        }

        var user = new User
        {
            Email = request.Email,
            Phone = request.Phone,
            Name = request.Name,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.Password),
            Role = request.Role ?? UserRole.Client,
            IsActive = true
        };

        await _userRepository.AddAsync(user);

        var userDto = MapToDto(user);
        return Result<UserDto>.Success(userDto);
    }

    public async Task<Result<LoginResponse>> LoginAsync(LoginRequest request)
    {
        var user = await _userRepository.GetByEmailAsync(request.Email);
        if (user == null || !BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash))
        {
            return Result<LoginResponse>.Failure("Invalid email or password");
        }

        if (!user.IsActive)
        {
            return Result<LoginResponse>.Failure("Account is deactivated");
        }

        var accessToken = _jwtService.GenerateAccessToken(user);
        var refreshToken = _jwtService.GenerateRefreshToken(user);

        var response = new LoginResponse(
            accessToken,
            refreshToken,
            MapToDto(user));

        return Result<LoginResponse>.Success(response);
    }

    public async Task<Result<LoginResponse>> RefreshTokenAsync(RefreshRequest request)
    {
        var principal = _jwtService.ValidateToken(request.RefreshToken);
        if (principal == null)
        {
            return Result<LoginResponse>.Failure("Invalid refresh token");
        }

        var userIdClaim = principal.FindFirst(ClaimTypes.NameIdentifier);
        if (userIdClaim == null || !Guid.TryParse(userIdClaim.Value, out var userId))
        {
            return Result<LoginResponse>.Failure("Invalid token claims");
        }

        var user = await _userRepository.GetByIdAsync(userId);
        if (user == null || !user.IsActive)
        {
            return Result<LoginResponse>.Failure("User not found or inactive");
        }

        var accessToken = _jwtService.GenerateAccessToken(user);
        var refreshToken = _jwtService.GenerateRefreshToken(user);

        var response = new LoginResponse(
            accessToken,
            refreshToken,
            MapToDto(user));

        return Result<LoginResponse>.Success(response);
    }

    private static UserDto MapToDto(User user)
    {
        return new UserDto(
            user.Id,
            user.Email,
            user.Phone,
            user.Name,
            user.Role,
            user.IsActive,
            user.CreatedAt);
    }

    public async Task<Result<UserDto>> GetByIdAsync(Guid userId)
    {
        var user = await _userRepository.GetByIdAsync(userId);
        if (user == null)
        {
            return Result<UserDto>.Failure("User not found");
        }

        return Result<UserDto>.Success(MapToDto(user));
    }
}