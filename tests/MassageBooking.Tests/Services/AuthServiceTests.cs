using Microsoft.Extensions.Configuration;
using FluentAssertions;
using MassageBooking.Application.DTOs;
using MassageBooking.Application.Interfaces;
using MassageBooking.Application.Services;
using MassageBooking.Domain.Common;
using MassageBooking.Domain.Entities;
using MassageBooking.Domain.Enums;
using MassageBooking.Domain.Interfaces;
using Moq;
using Xunit;

namespace MassageBooking.Tests.Services;

public class AuthServiceTests
{
    private readonly Mock<IUserRepository> _userRepoMock;
    private readonly Mock<IJwtService> _jwtServiceMock;
    private readonly Mock<IConfiguration> _configMock;
    private readonly AuthService _sut;

    public AuthServiceTests()
    {
        _userRepoMock = new Mock<IUserRepository>();
        _jwtServiceMock = new Mock<IJwtService>();
        _configMock = new Mock<IConfiguration>();
        _sut = new AuthService(_userRepoMock.Object, _jwtServiceMock.Object, _configMock.Object);
    }

    [Fact]
    public async Task RegisterAsync_ValidRequest_CreatesUser()
    {
        // Arrange
        var request = new RegisterRequest("test@example.com", "password123", "1234567890", "Test User");
        _userRepoMock.Setup(x => x.GetByEmailAsync(request.Email))
            .ReturnsAsync((User?)null);
        _userRepoMock.Setup(x => x.AddAsync(It.IsAny<User>()))
            .ReturnsAsync(new User { Id = Guid.NewGuid() });

        // Act
        var result = await _sut.RegisterAsync(request);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value!.Email.Should().Be("test@example.com");
    }

    [Fact]
    public async Task RegisterAsync_DuplicateEmail_ReturnsFailure()
    {
        // Arrange
        var request = new RegisterRequest("test@example.com", "password123", "1234567890", "Test User");
        var existingUser = new User { Email = request.Email };
        _userRepoMock.Setup(x => x.GetByEmailAsync(request.Email))
            .ReturnsAsync(existingUser);

        // Act
        var result = await _sut.RegisterAsync(request);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Be("Email already registered");
    }

    [Fact]
    public async Task LoginAsync_ValidCredentials_ReturnsTokens()
    {
        // Arrange
        var request = new LoginRequest("test@example.com", "password123");
        var user = new User
        {
            Id = Guid.NewGuid(),
            Email = request.Email,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.Password),
            IsActive = true
        };
        _userRepoMock.Setup(x => x.GetByEmailAsync(request.Email)).ReturnsAsync(user);
        _jwtServiceMock.Setup(x => x.GenerateAccessToken(user))
            .Returns("access-token");
        _jwtServiceMock.Setup(x => x.GenerateRefreshToken(user))
            .Returns("refresh-token");

        // Act
        var result = await _sut.LoginAsync(request);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value!.AccessToken.Should().Be("access-token");
    }

    [Fact]
    public async Task LoginAsync_InvalidPassword_ReturnsFailure()
    {
        // Arrange
        var request = new LoginRequest("test@example.com", "wrongpassword");
        var user = new User
        {
            Id = Guid.NewGuid(),
            Email = request.Email,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword("correctpassword"),
            IsActive = true
        };
        _userRepoMock.Setup(x => x.GetByEmailAsync(request.Email)).ReturnsAsync(user);

        // Act
        var result = await _sut.LoginAsync(request);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Be("Invalid email or password");
    }
}
