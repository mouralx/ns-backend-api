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

    public async Task<Result<DashboardStatsDto>> GetStatsAsync()
    {
        var allAppointments = await _appointmentRepository.GetAllAsync();
        var today = DateTime.UtcNow.Date;

        var todayAppointments = allAppointments.Where(a =>
            a.ScheduledAt.Date == today &&
            a.Status != AppointmentStatus.Cancelled).ToList();

        var totalClients = (await _userRepository.GetAllAsync())
            .Count(u => u.Role == UserRole.Client);

        var totalTherapists = (await _userRepository.GetAllAsync())
            .Count(u => u.Role == UserRole.Therapist);

        var totalServices = (await _serviceTypeRepository.GetAllAsync())
            .Count(s => s.IsActive);

        var stats = new DashboardStatsDto(
            todayAppointments.Count,
            totalClients,
            totalTherapists,
            totalServices,
            todayAppointments.Count(a => a.Status == AppointmentStatus.Pending),
            todayAppointments.Count(a => a.Status == AppointmentStatus.Confirmed));

        return Result<DashboardStatsDto>.Success(stats);
    }

    public async Task<Result<IEnumerable<AppointmentDto>>> GetTodayScheduleAsync()
    {
        var today = DateTime.UtcNow.Date;
        var tomorrow = today.AddDays(1);
        var appointments = await _appointmentRepository.GetByDateRangeAsync(today, tomorrow);

        var dtos = appointments
            .Where(a => a.Status != AppointmentStatus.Cancelled)
            .OrderBy(a => a.ScheduledAt)
            .Select(a => new AppointmentDto(
                a.Id,
                a.ClientId,
                null!,
                a.TherapistId,
                null!,
                a.ServiceTypeId,
                null!,
                a.ScheduledAt,
                a.DurationMin,
                a.Status,
                a.ConfirmationStatus,
                a.ConfirmedAt,
                a.CancelledAt,
                a.CancellationReason,
                a.Notes,
                a.IsWalkin,
                a.CreatedAt)).ToList();

        return Result<IEnumerable<AppointmentDto>>.Success(dtos);
    }

    public async Task<Result<IEnumerable<AppointmentDto>>> GetUpcomingAsync(int limit = 10)
    {
        var now = DateTime.UtcNow;
        var futureDate = now.AddDays(30);
        var appointments = await _appointmentRepository.GetByDateRangeAsync(now, futureDate);

        var dtos = appointments
            .Where(a => a.Status == AppointmentStatus.Pending || a.Status == AppointmentStatus.Confirmed)
            .OrderBy(a => a.ScheduledAt)
            .Take(limit)
            .Select(a => new AppointmentDto(
                a.Id,
                a.ClientId,
                null!,
                a.TherapistId,
                null!,
                a.ServiceTypeId,
                null!,
                a.ScheduledAt,
                a.DurationMin,
                a.Status,
                a.ConfirmationStatus,
                a.ConfirmedAt,
                a.CancelledAt,
                a.CancellationReason,
                a.Notes,
                a.IsWalkin,
                a.CreatedAt)).ToList();

        return Result<IEnumerable<AppointmentDto>>.Success(dtos);
    }

    public async Task<Result<IEnumerable<AppointmentDto>>> GetAtRiskAppointmentsAsync()
    {
        var now = DateTime.UtcNow;
        var fourHoursFromNow = now.AddHours(4);
        var tomorrow = now.Date.AddDays(1);

        var appointments = await _appointmentRepository.GetByDateRangeAsync(now, tomorrow);

        var atRisk = appointments
            .Where(a =>
                a.Status == AppointmentStatus.Pending &&
                a.ConfirmationStatus != ConfirmationStatus.Confirmed &&
                a.ScheduledAt <= fourHoursFromNow)
            .OrderBy(a => a.ScheduledAt)
            .Select(a => new AppointmentDto(
                a.Id,
                a.ClientId,
                null!,
                a.TherapistId,
                null!,
                a.ServiceTypeId,
                null!,
                a.ScheduledAt,
                a.DurationMin,
                a.Status,
                ConfirmationStatus.AtRisk,
                a.ConfirmedAt,
                a.CancelledAt,
                a.CancellationReason,
                a.Notes,
                a.IsWalkin,
                a.CreatedAt)).ToList();

        return Result<IEnumerable<AppointmentDto>>.Success(atRisk);
    }
}
