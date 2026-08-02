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

    public NotificationService(INotificationRepository notificationRepository)
    {
        _notificationRepository = notificationRepository;
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
}
