using MassageBooking.Application.DTOs;
using MassageBooking.Application.Interfaces;
using MassageBooking.Domain.Common;
using MassageBooking.Domain.Enums;
using MassageBooking.Domain.Interfaces;

namespace MassageBooking.Application.Services;

public class DashboardService : IDashboardService
{
    private readonly IAppointmentRepository _appointmentRepository;
    private readonly IUserRepository _userRepository;
    private readonly IServiceTypeRepository _serviceTypeRepository;

    public DashboardService(
        IAppointmentRepository appointmentRepository,
        IUserRepository userRepository,
        IServiceTypeRepository serviceTypeRepository)
    {
        _appointmentRepository = appointmentRepository;
        _userRepository = userRepository;
        _serviceTypeRepository = serviceTypeRepository;
    }

    public async Task<Result<TodayScheduleDto>> GetTodayScheduleAsync(Guid therapistId)
    {
        var today = DateTime.UtcNow.Date;
        var tomorrow = today.AddDays(1);

        var appointments = await _appointmentRepository.GetByDateRangeAsync(today, tomorrow);
        var therapistAppointments = appointments
            .Where(a => a.TherapistId == therapistId && a.Status != AppointmentStatus.Cancelled)
            .OrderBy(a => a.ScheduledAt);

        var dtos = new List<AppointmentDto>();
        foreach (var appointment in therapistAppointments)
        {
            var client = await _userRepository.GetByIdAsync(appointment.ClientId);
            var serviceType = await _serviceTypeRepository.GetByIdAsync(appointment.ServiceTypeId);

            dtos.Add(new AppointmentDto(
                appointment.Id,
                appointment.ClientId,
                client != null ? new UserDto(client.Id, client.Email, client.Phone, client.Name, client.Role, client.IsActive, client.CreatedAt) : null!,
                appointment.TherapistId,
                null!,
                appointment.ServiceTypeId,
                serviceType != null ? new ServiceTypeDto(serviceType.Id, serviceType.Name, serviceType.DurationMin, serviceType.Description, serviceType.IsActive) : null!,
                appointment.ScheduledAt,
                appointment.DurationMin,
                appointment.Status,
                appointment.ConfirmationStatus,
                appointment.ConfirmedAt,
                appointment.CancelledAt,
                appointment.CancellationReason,
                appointment.Notes,
                appointment.IsWalkin,
                appointment.CreatedAt));
        }

        return Result<TodayScheduleDto>.Success(new TodayScheduleDto(dtos));
    }

    public async Task<Result<DashboardStatsDto>> GetStatsAsync(Guid therapistId)
    {
        var thirtyDaysAgo = DateTime.UtcNow.AddDays(-30);
        var appointments = await _appointmentRepository.GetByDateRangeAsync(thirtyDaysAgo, DateTime.UtcNow);
        var therapistAppointments = appointments.Where(a => a.TherapistId == therapistId).ToList();

        var totalBooked = therapistAppointments.Count;
        var confirmed = therapistAppointments.Count(a => a.Status == AppointmentStatus.Confirmed);
        var cancelled = therapistAppointments.Count(a => a.Status == AppointmentStatus.Cancelled);
        var noShow = therapistAppointments.Count(a => a.Status == AppointmentStatus.NoShow);
        var confirmationRate = totalBooked > 0 ? (double)confirmed / totalBooked * 100 : 0;

        return Result<DashboardStatsDto>.Success(new DashboardStatsDto(
            totalBooked,
            confirmed,
            cancelled,
            noShow,
            Math.Round(confirmationRate, 2)));
    }

    public async Task<Result<List<AtRiskAppointmentDto>>> GetAtRiskAppointmentsAsync(Guid therapistId)
    {
        var now = DateTime.UtcNow;
        var tomorrow = now.AddDays(1);

        var appointments = await _appointmentRepository.GetByDateRangeAsync(now, tomorrow);
        var atRisk = appointments
            .Where(a => a.TherapistId == therapistId &&
                       a.ConfirmationStatus == ConfirmationStatus.Unconfirmed &&
                       a.Status != AppointmentStatus.Cancelled)
            .ToList();

        var dtos = new List<AtRiskAppointmentDto>();
        foreach (var appointment in atRisk)
        {
            var client = await _userRepository.GetByIdAsync(appointment.ClientId);
            var serviceType = await _serviceTypeRepository.GetByIdAsync(appointment.ServiceTypeId);

            dtos.Add(new AtRiskAppointmentDto(
                appointment.Id,
                client?.Name ?? "Unknown",
                serviceType?.Name ?? "Unknown",
                appointment.ScheduledAt,
                appointment.ScheduledAt.AddHours(-4)));
        }

        return Result<List<AtRiskAppointmentDto>>.Success(dtos);
    }
}
