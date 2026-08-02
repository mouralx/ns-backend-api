using FluentAssertions;
using MassageBooking.Application.Services;
using MassageBooking.Domain.Entities;
using MassageBooking.Domain.Enums;
using MassageBooking.Domain.Interfaces;
using Moq;
using Xunit;

namespace MassageBooking.Tests.Services;

public class DashboardServiceTests
{
    private readonly Mock<IAppointmentRepository> _appointmentRepoMock;
    private readonly Mock<IUserRepository> _userRepoMock;
    private readonly Mock<IServiceTypeRepository> _serviceTypeRepoMock;
    private readonly DashboardService _sut;

    public DashboardServiceTests()
    {
        _appointmentRepoMock = new Mock<IAppointmentRepository>();
        _userRepoMock = new Mock<IUserRepository>();
        _serviceTypeRepoMock = new Mock<IServiceTypeRepository>();
        _sut = new DashboardService(
            _appointmentRepoMock.Object,
            _userRepoMock.Object,
            _serviceTypeRepoMock.Object);
    }

    [Fact]
    public async Task GetStatsAsync_ReturnsCorrectCounts()
    {
        // Arrange
        var today = DateTime.UtcNow.Date;
        var appointments = new List<Appointment>
        {
            new() { ScheduledAt = today.AddHours(10), Status = AppointmentStatus.Pending },
            new() { ScheduledAt = today.AddHours(14), Status = AppointmentStatus.Confirmed },
            new() { ScheduledAt = today.AddDays(1), Status = AppointmentStatus.Pending }
        };
        var users = new List<User>
        {
            new() { Role = UserRole.Client },
            new() { Role = UserRole.Client },
            new() { Role = UserRole.Therapist }
        };
        var services = new List<ServiceType>
        {
            new() { IsActive = true },
            new() { IsActive = false }
        };

        _appointmentRepoMock.Setup(x => x.GetAllAsync()).ReturnsAsync(appointments);
        _userRepoMock.Setup(x => x.GetAllAsync()).ReturnsAsync(users);
        _serviceTypeRepoMock.Setup(x => x.GetAllAsync()).ReturnsAsync(services);

        // Act
        var result = await _sut.GetStatsAsync();

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value!.TodayAppointments.Should().Be(2);
        result.Value.TotalClients.Should().Be(2);
        result.Value.TotalTherapists.Should().Be(1);
        result.Value.TotalServices.Should().Be(1);
    }

    [Fact]
    public async Task GetAtRiskAppointmentsAsync_ReturnsUnconfirmedUpcoming()
    {
        // Arrange
        var now = DateTime.UtcNow;
        var appointments = new List<Appointment>
        {
            new()
            {
                Id = Guid.NewGuid(),
                ScheduledAt = now.AddHours(2),
                Status = AppointmentStatus.Pending,
                ConfirmationStatus = ConfirmationStatus.Unconfirmed,
                DurationMin = 60,
                CreatedAt = now
            },
            new()
            {
                Id = Guid.NewGuid(),
                ScheduledAt = now.AddHours(8),
                Status = AppointmentStatus.Pending,
                ConfirmationStatus = ConfirmationStatus.Confirmed,
                DurationMin = 60,
                CreatedAt = now
            }
        };

        _appointmentRepoMock.Setup(x => x.GetByDateRangeAsync(
            It.IsAny<DateTime>(), It.IsAny<DateTime>()))
            .ReturnsAsync(appointments);

        // Act
        var result = await _sut.GetAtRiskAppointmentsAsync();

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value!.Count().Should().Be(1);
    }
}
