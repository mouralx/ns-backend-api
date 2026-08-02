using MassageBooking.Application.DTOs;
using MassageBooking.Domain.Common;

namespace MassageBooking.Application.Interfaces;

public interface INotificationService
{
    Task<Result<IEnumerable<NotificationDto>>> GetByUserAsync(Guid userId);
    Task<Result> MarkAsReadAsync(Guid userId, MarkReadRequest request);
    Task<Result<NotificationDto>> GetByIdAsync(Guid id);
}
