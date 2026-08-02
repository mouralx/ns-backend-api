using MassageBooking.Domain.Entities;
using MassageBooking.Domain.Enums;

namespace MassageBooking.Domain.Interfaces;

public interface INotificationRepository
{
    Task<Notification?> GetByIdAsync(Guid id);
    Task<IEnumerable<Notification>> GetByUserAsync(Guid userId);
    Task<IEnumerable<Notification>> GetAllAsync();
    Task<Notification> AddAsync(Notification notification);
    Task<Notification> UpdateAsync(Notification notification);
    Task<Notification?> GetByAppointmentAndTypeAsync(Guid appointmentId, NotificationType type);
}
