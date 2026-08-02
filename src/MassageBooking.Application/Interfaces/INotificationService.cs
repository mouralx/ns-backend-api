using MassageBooking.Application.DTOs;
using MassageBooking.Domain.Common;

namespace MassageBooking.Application.Interfaces;

public interface INotificationService
{
    Task<Result<IEnumerable<NotificationDto>>> GetByUserAsync(Guid userId);
    Task<Result> MarkAsReadAsync(Guid userId, MarkReadRequest request);
    Task<Result<NotificationDto>> GetByIdAsync(Guid id);
    Task CreateBookingNotificationAsync(Guid appointmentId, Guid clientId, Guid therapistId, string serviceName, DateTime scheduledAt);
    Task CreateCancellationNotificationAsync(Guid appointmentId, Guid clientId, Guid therapistId);
    Task CreateConfirmedNotificationAsync(Guid appointmentId, Guid clientId, Guid therapistId);
    Task CreateReminderNotificationAsync(Guid appointmentId, Guid userId, string serviceName, DateTime scheduledAt);
}
