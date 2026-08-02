using MassageBooking.Domain.Entities;
using MassageBooking.Domain.Enums;
using MassageBooking.Domain.Interfaces;
using MassageBooking.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace MassageBooking.Infrastructure.Repositories;

public class NotificationRepository : INotificationRepository
{
    private readonly AppDbContext _context;

    public NotificationRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<Notification?> GetByIdAsync(Guid id)
    {
        return await _context.Notifications.FindAsync(id);
    }

    public async Task<IEnumerable<Notification>> GetByUserAsync(Guid userId)
    {
        return await _context.Notifications
            .Where(n => n.UserId == userId)
            .OrderByDescending(n => n.CreatedAt)
            .ToListAsync();
    }

    public async Task<IEnumerable<Notification>> GetAllAsync()
    {
        return await _context.Notifications.ToListAsync();
    }

    public async Task<Notification> AddAsync(Notification notification)
    {
        await _context.Notifications.AddAsync(notification);
        await _context.SaveChangesAsync();
        return notification;
    }

    public async Task<Notification> UpdateAsync(Notification notification)
    {
        _context.Notifications.Update(notification);
        await _context.SaveChangesAsync();
        return notification;
    }

    public async Task<Notification?> GetByAppointmentAndTypeAsync(Guid appointmentId, NotificationType type)
    {
        return await _context.Notifications
            .FirstOrDefaultAsync(n => n.AppointmentId == appointmentId && n.Type == type);
    }
}
