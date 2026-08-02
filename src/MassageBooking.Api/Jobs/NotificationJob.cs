using MassageBooking.Application.Interfaces;
using MassageBooking.Domain.Enums;
using MassageBooking.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace MassageBooking.Api.Jobs;

public class NotificationJob
{
    private readonly IServiceProvider _serviceProvider;

    public NotificationJob(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    /// <summary>
    /// Sends 24-hour confirmation requests for appointments happening tomorrow.
    /// Runs daily at midnight.
    /// </summary>
    public async Task Send24HourConfirmationsAsync()
    {
        using var scope = _serviceProvider.CreateScope();
        var appointmentRepository = scope.ServiceProvider.GetRequiredService<IAppointmentRepository>();
        var userRepository = scope.ServiceProvider.GetRequiredService<IUserRepository>();
        var notificationService = scope.ServiceProvider.GetRequiredService<INotificationService>();

        var tomorrow = DateTime.UtcNow.Date.AddDays(1);
        var dayAfterTomorrow = tomorrow.AddDays(1);

        var appointments = await appointmentRepository.GetByDateRangeAsync(tomorrow, dayAfterTomorrow);

        foreach (var appointment in appointments.Where(a =>
            a.Status == AppointmentStatus.Scheduled &&
            a.ConfirmationStatus != ConfirmationStatus.Confirmed))
        {
            var client = await userRepository.GetByIdAsync(appointment.ClientId);
            if (client?.PushToken != null)
            {
                await notificationService.CreateReminderNotificationAsync(
                    appointment.Id,
                    appointment.ClientId,
                    appointment.ServiceType?.Name ?? "appointment",
                    appointment.ScheduledAt);
            }
        }
    }

    /// <summary>
    /// Sends 2-hour reminders for appointments happening soon.
    /// Runs every 4 hours.
    /// </summary>
    public async Task Send2HourRemindersAsync()
    {
        using var scope = _serviceProvider.CreateScope();
        var appointmentRepository = scope.ServiceProvider.GetRequiredService<IAppointmentRepository>();
        var userRepository = scope.ServiceProvider.GetRequiredService<IUserRepository>();
        var notificationService = scope.ServiceProvider.GetRequiredService<INotificationService>();

        var now = DateTime.UtcNow;
        var twoHoursFromNow = now.AddHours(2);

        var appointments = await appointmentRepository.GetByDateRangeAsync(now, twoHoursFromNow);

        foreach (var appointment in appointments.Where(a =>
            a.Status == AppointmentStatus.Confirmed))
        {
            // Remind client
            await notificationService.CreateReminderNotificationAsync(
                appointment.Id,
                appointment.ClientId,
                appointment.ServiceType?.Name ?? "appointment",
                appointment.ScheduledAt);

            // Remind therapist
            await notificationService.CreateReminderNotificationAsync(
                appointment.Id,
                appointment.TherapistId,
                appointment.ServiceType?.Name ?? "appointment",
                appointment.ScheduledAt);
        }
    }

    /// <summary>
    /// Detects and marks no-show appointments.
    /// Runs every hour.
    /// </summary>
    public async Task DetectNoShowsAsync()
    {
        using var scope = _serviceProvider.CreateScope();
        var appointmentRepository = scope.ServiceProvider.GetRequiredService<IAppointmentRepository>();

        var now = DateTime.UtcNow;
        var oneHourAgo = now.AddHours(-1);

        var appointments = await appointmentRepository.GetByDateRangeAsync(oneHourAgo, now);

        foreach (var appointment in appointments.Where(a =>
            a.Status == AppointmentStatus.Scheduled &&
            a.ScheduledAt.AddMinutes(a.DurationMin) < now))
        {
            appointment.Status = AppointmentStatus.NoShow;
            appointment.UpdatedAt = now;
            await appointmentRepository.UpdateAsync(appointment);
        }
    }
}