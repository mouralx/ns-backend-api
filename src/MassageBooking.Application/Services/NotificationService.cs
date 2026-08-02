using MassageBooking.Application.DTOs;
using MassageBooking.Application.Interfaces;
using MassageBooking.Domain.Common;
using MassageBooking.Domain.Entities;
using MassageBooking.Domain.Enums;
using MassageBooking.Domain.Interfaces;

namespace MassageBooking.Application.Services;

public class NotificationService : INotificationService
{
    private readonly INotificationRepository _notificationRepository;
    private readonly IPushNotificationService _pushNotificationService;
    private readonly IUserRepository _userRepository;

    public NotificationService(
        INotificationRepository notificationRepository,
        IPushNotificationService pushNotificationService,
        IUserRepository userRepository)
    {
        _notificationRepository = notificationRepository;
        _pushNotificationService = pushNotificationService;
        _userRepository = userRepository;
    }

    public async Task<Result<IEnumerable<NotificationDto>>> GetByUserAsync(Guid userId)
    {
        var notifications = await _notificationRepository.GetByUserAsync(userId);
        var dtos = notifications.Select(n => new NotificationDto(
            n.Id,
            n.Type,
            n.Channel,
            n.Status,
            n.SentAt,
            n.ReadAt,
            n.AppointmentId,
            n.ErrorMessage ?? "")).ToList();

        return Result<IEnumerable<NotificationDto>>.Success(dtos);
    }

    public async Task<Result> MarkAsReadAsync(Guid userId, MarkReadRequest request)
    {
        foreach (var notificationId in request.NotificationIds)
        {
            var notification = await _notificationRepository.GetByIdAsync(notificationId);
            if (notification != null && notification.UserId == userId)
            {
                notification.Status = NotificationStatus.Read;
                notification.ReadAt = DateTime.UtcNow;
                notification.UpdatedAt = DateTime.UtcNow;
                await _notificationRepository.UpdateAsync(notification);
            }
        }

        return Result.Success();
    }

    public async Task<Result<NotificationDto>> GetByIdAsync(Guid id)
    {
        var notification = await _notificationRepository.GetByIdAsync(id);
        if (notification == null)
        {
            return Result<NotificationDto>.Failure("Notification not found");
        }

        var dto = new NotificationDto(
            notification.Id,
            notification.Type,
            notification.Channel,
            notification.Status,
            notification.SentAt,
            notification.ReadAt,
            notification.AppointmentId,
            notification.ErrorMessage ?? "");

        return Result<NotificationDto>.Success(dto);
    }

    public async Task CreateBookingNotificationAsync(Guid appointmentId, Guid clientId, Guid therapistId, string serviceName, DateTime scheduledAt)
    {
        // Notify therapist
        var therapist = await _userRepository.GetByIdAsync(therapistId);
        if (therapist?.PushToken != null)
        {
            var notification = new Notification
            {
                UserId = therapistId,
                AppointmentId = appointmentId,
                Type = NotificationType.BookingCreated,
                Channel = NotificationChannel.Push,
                Status = NotificationStatus.Pending
            };
            await _notificationRepository.AddAsync(notification);

            try
            {
                await _pushNotificationService.SendAsync(
                    therapist.PushToken,
                    "New Booking",
                    $"New {serviceName} appointment on {scheduledAt:MMM dd, HH:mm}");
                notification.Status = NotificationStatus.Sent;
                notification.SentAt = DateTime.UtcNow;
            }
            catch (Exception ex)
            {
                notification.Status = NotificationStatus.Failed;
                notification.ErrorMessage = ex.Message;
            }
            await _notificationRepository.UpdateAsync(notification);
        }

        // Notify client
        var client = await _userRepository.GetByIdAsync(clientId);
        if (client?.PushToken != null)
        {
            var notification = new Notification
            {
                UserId = clientId,
                AppointmentId = appointmentId,
                Type = NotificationType.BookingCreated,
                Channel = NotificationChannel.Push,
                Status = NotificationStatus.Pending
            };
            await _notificationRepository.AddAsync(notification);

            try
            {
                await _pushNotificationService.SendAsync(
                    client.PushToken,
                    "Booking Confirmed",
                    $"Your {serviceName} is scheduled for {scheduledAt:MMM dd, HH:mm}");
                notification.Status = NotificationStatus.Sent;
                notification.SentAt = DateTime.UtcNow;
            }
            catch (Exception ex)
            {
                notification.Status = NotificationStatus.Failed;
                notification.ErrorMessage = ex.Message;
            }
            await _notificationRepository.UpdateAsync(notification);
        }
    }

    public async Task CreateCancellationNotificationAsync(Guid appointmentId, Guid clientId, Guid therapistId)
    {
        var therapist = await _userRepository.GetByIdAsync(therapistId);
        if (therapist?.PushToken != null)
        {
            var notification = new Notification
            {
                UserId = therapistId,
                AppointmentId = appointmentId,
                Type = NotificationType.AppointmentCancelled,
                Channel = NotificationChannel.Push,
                Status = NotificationStatus.Pending
            };
            await _notificationRepository.AddAsync(notification);

            try
            {
                await _pushNotificationService.SendAsync(
                    therapist.PushToken,
                    "Appointment Cancelled",
                    "An appointment has been cancelled");
                notification.Status = NotificationStatus.Sent;
                notification.SentAt = DateTime.UtcNow;
            }
            catch (Exception ex)
            {
                notification.Status = NotificationStatus.Failed;
                notification.ErrorMessage = ex.Message;
            }
            await _notificationRepository.UpdateAsync(notification);
        }
    }

    public async Task CreateConfirmedNotificationAsync(Guid appointmentId, Guid clientId, Guid therapistId)
    {
        var client = await _userRepository.GetByIdAsync(clientId);
        if (client?.PushToken != null)
        {
            var notification = new Notification
            {
                UserId = clientId,
                AppointmentId = appointmentId,
                Type = NotificationType.AppointmentConfirmed,
                Channel = NotificationChannel.Push,
                Status = NotificationStatus.Pending
            };
            await _notificationRepository.AddAsync(notification);

            try
            {
                await _pushNotificationService.SendAsync(
                    client.PushToken,
                    "Appointment Confirmed",
                    "Your appointment has been confirmed");
                notification.Status = NotificationStatus.Sent;
                notification.SentAt = DateTime.UtcNow;
            }
            catch (Exception ex)
            {
                notification.Status = NotificationStatus.Failed;
                notification.ErrorMessage = ex.Message;
            }
            await _notificationRepository.UpdateAsync(notification);
        }
    }

    public async Task CreateReminderNotificationAsync(Guid appointmentId, Guid userId, string serviceName, DateTime scheduledAt)
    {
        var user = await _userRepository.GetByIdAsync(userId);
        if (user?.PushToken != null)
        {
            var notification = new Notification
            {
                UserId = userId,
                AppointmentId = appointmentId,
                Type = NotificationType.AppointmentReminder,
                Channel = NotificationChannel.Push,
                Status = NotificationStatus.Pending
            };
            await _notificationRepository.AddAsync(notification);

            try
            {
                await _pushNotificationService.SendAsync(
                    user.PushToken,
                    "Appointment Reminder",
                    $"Reminder: {serviceName} at {scheduledAt:HH:mm} tomorrow");
                notification.Status = NotificationStatus.Sent;
                notification.SentAt = DateTime.UtcNow;
            }
            catch (Exception ex)
            {
                notification.Status = NotificationStatus.Failed;
                notification.ErrorMessage = ex.Message;
            }
            await _notificationRepository.UpdateAsync(notification);
        }
    }
}
