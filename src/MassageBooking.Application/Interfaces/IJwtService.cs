using System.Security.Claims;
using MassageBooking.Domain.Entities;

namespace MassageBooking.Application.Interfaces;

public interface IJwtService
{
    string GenerateAccessToken(User user);
    string GenerateRefreshToken(User user);
    ClaimsPrincipal? ValidateToken(string token);
}
