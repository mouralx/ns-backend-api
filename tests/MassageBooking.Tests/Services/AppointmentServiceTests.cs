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

public class AppointmentServiceTests
{
    private readonly Mock<IAppointmentRepository> _appointmentRepoMock;
    private readonly Mock<IUserRepository> _userRepoMock;
    private readonly Mock<IServiceTypeRepository> _serviceTypeRepoMock;
    private readonly Mock<IAvailabilityRepository> _availabilityRepoMock;
    private readonly Mock<INotificationService> _notificationServiceMock;
    private readonly AppointmentService _sut;

    public AppointmentServiceTests()
    {
        _appointmentRepoMock = new Mock<IAppointmentRepository>();
        _userRepoMock = new Mock<IUserRepository>();
        _serviceTypeRepoMock = new Mock<IServiceTypeRepository>();
        _availabilityRepoMock = new Mock<IAvailabilityRepository>();
        _notificationServiceMock = new Mock<INotificationService>();
        _sut = new AppointmentService(
            _appointmentRepoMock.Object,
            _userRepoMock.Object,
            _serviceTypeRepoMock.Object,
            _availabilityRepoMock.Object,
            _notificationServiceMock.Object);
    }

    [Fact]
    public async Task GetByIdAsync_ExistingAppointment_ReturnsSuccess()
    {
        // Arrange
        var appointmentId = Guid.NewGuid();
        var appointment = CreateTestAppointment(appointmentId);
        _appointmentRepoMock.Setup(x => x.GetByIdAsync(appointmentId))
            .ReturnsAsync(appointment);
        _userRepoMock.Setup(x => x.GetByIdAsync(It.IsAny<Guid>()))
            .ReturnsAsync(new User { Id = Guid.NewGuid(), Name = "Test" });
        _serviceTypeRepoMock.Setup(x => x.GetByIdAsync(It.IsAny<Guid>()))
            .ReturnsAsync(new ServiceType { Id = Guid.NewGuid(), Name = "Massage" });

        // Act
        var result = await _sut.GetByIdAsync(appointmentId);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value!.Id.Should().Be(appointmentId);
    }

    [Fact]
    public async Task GetByIdAsync_NonExistingAppointment_ReturnsFailure()
    {
        // Arrange
        var appointmentId = Guid.NewGuid();
        _appointmentRepoMock.Setup(x => x.GetByIdAsync(appointmentId))
            .ReturnsAsync((Appointment?)null);

        // Act
        var result = await _sut.GetByIdAsync(appointmentId);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Be("Appointment not found");
    }

    [Fact]
    public async Task BookAsync_ValidRequest_CreatesAppointment()
    {
        // Arrange
        var clientId = Guid.NewGuid();
        var therapistId = Guid.NewGuid();
        var serviceTypeId = Guid.NewGuid();

        var therapist = new User { Id = therapistId, Role = UserRole.Therapist, IsActive = true };
        var serviceType = new ServiceType { Id = serviceTypeId, IsActive = true, DurationMin = 60 };
        var request = new BookAppointmentRequest(therapistId, serviceTypeId, DateTime.UtcNow.AddHours(2), null);

        _userRepoMock.Setup(x => x.GetByIdAsync(therapistId)).ReturnsAsync(therapist);
        _serviceTypeRepoMock.Setup(x => x.GetByIdAsync(serviceTypeId)).ReturnsAsync(serviceType);
        _appointmentRepoMock.Setup(x => x.GetConflictingAsync(
            It.IsAny<Guid>(), It.IsAny<DateTime>(), It.IsAny<DateTime>()))
            .ReturnsAsync(new List<Appointment>());
        _appointmentRepoMock.Setup(x => x.AddAsync(It.IsAny<Appointment>()))
            .Returns(Task.CompletedTask);
        _appointmentRepoMock.Setup(x => x.GetByIdAsync(It.IsAny<Guid>()))
            .ReturnsAsync((Guid id) => CreateTestAppointment(id));

        // Act
        var result = await _sut.BookAsync(request, clientId);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        _notificationServiceMock.Verify(x => x.CreateBookingNotificationAsync(
            It.IsAny<Guid>(), clientId, therapistId, It.IsAny<string>(), It.IsAny<DateTime>()),
            Times.Once);
    }

    [Fact]
    public async Task BookAsync_ConflictingSlot_ReturnsFailure()
    {
        // Arrange
        var clientId = Guid.NewGuid();
        var therapistId = Guid.NewGuid();
        var serviceTypeId = Guid.NewGuid();

        var therapist = new User { Id = therapistId, Role = UserRole.Therapist, IsActive = true };
        var serviceType = new ServiceType { Id = serviceTypeId, IsActive = true, DurationMin = 60 };
        var request = new BookAppointmentRequest(therapistId, serviceTypeId, DateTime.UtcNow.AddHours(2), null);

        _userRepoMock.Setup(x => x.GetByIdAsync(therapistId)).ReturnsAsync(therapist);
        _serviceTypeRepoMock.Setup(x => x.GetByIdAsync(serviceTypeId)).ReturnsAsync(serviceType);
        _appointmentRepoMock.Setup(x => x.GetConflictingAsync(
            It.IsAny<Guid>(), It.IsAny<DateTime>(), It.IsAny<DateTime>()))
            .ReturnsAsync(new List<Appointment> { new Appointment { Id = Guid.NewGuid() } });

        // Act
        var result = await _sut.BookAsync(request, clientId);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Be("Time slot is not available");
    }

    [Fact]
    public async Task CancelAsync_AfterCancellationWindow_ReturnsFailure()
    {
        // Arrange
        var appointmentId = Guid.NewGuid();
        var appointment = CreateTestAppointment(appointmentId);
        appointment.ScheduledAt = DateTime.UtcNow.AddHours(2); // Less than 4 hours

        _appointmentRepoMock.Setup(x => x.GetByIdAsync(appointmentId))
            .ReturnsAsync(appointment);

        // Act
        var result = await _sut.CancelAsync(appointmentId);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Contain("Cannot cancel within 4 hours");
    }

    [Fact]
    public async Task ConfirmAsync_ExistingAppointment_ConfirmsSuccessfully()
    {
        // Arrange
        var appointmentId = Guid.NewGuid();
        var appointment = CreateTestAppointment(appointmentId);
        appointment.Status = AppointmentStatus.Scheduled;

        _appointmentRepoMock.Setup(x => x.GetByIdAsync(appointmentId))
            .ReturnsAsync(appointment);
        _appointmentRepoMock.Setup(x => x.UpdateAsync(It.IsAny<Appointment>()))
            .Returns(Task.CompletedTask);
        _userRepoMock.Setup(x => x.GetByIdAsync(It.IsAny<Guid>()))
            .ReturnsAsync(new User { Id = Guid.NewGuid(), PushToken = "token" });

        // Act
        var result = await _sut.ConfirmAsync(appointmentId);

        // Assert
        result.IsSuccess.Should().BeTrue();
        _notificationServiceMock.Verify(x => x.CreateConfirmedNotificationAsync(
            It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<Guid>()),
            Times.Once);
    }

    private Appointment CreateTestAppointment(Guid id)
    {
        return new Appointment
        {
            Id = id,
            ClientId = Guid.NewGuid(),
            TherapistId = Guid.NewGuid(),
            ServiceTypeId = Guid.NewGuid(),
            ScheduledAt = DateTime.UtcNow.AddHours(8),
            DurationMin = 60,
            Status = AppointmentStatus.Scheduled,
            ConfirmationStatus = ConfirmationStatus.Pending,
            Notes = null,
            IsWalkin = false,
            CreatedAt = DateTime.UtcNow
        };
    }
}