using MassageBooking.Application.DTOs;
using MassageBooking.Domain.Common;

namespace MassageBooking.Application.Interfaces;
public interface IAuthService
{
    Task<Result<UserDto>> RegisterAsync(RegisterRequest request);
    Task<Result<LoginResponse>> LoginAsync(LoginRequest request);
    Task<Result<LoginResponse>> RefreshTokenAsync(RefreshRequest request);
    Task<Result<UserDto>> GetByIdAsync(Guid userId);
}