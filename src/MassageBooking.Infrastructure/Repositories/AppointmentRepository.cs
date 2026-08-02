using MassageBooking.Domain.Entities;
using MassageBooking.Domain.Interfaces;
using MassageBooking.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace MassageBooking.Infrastructure.Repositories;

public class AppointmentRepository : IAppointmentRepository
{
    private readonly AppDbContext _context;

    public AppointmentRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<Appointment?> GetByIdAsync(Guid id)
    {
        return await _context.Appointments
            .Include(a => a.Client)
            .Include(a => a.Therapist)
            .Include(a => a.ServiceType)
            .FirstOrDefaultAsync(a => a.Id == id);
    }

    public async Task<IEnumerable<Appointment>> GetAllAsync()
    {
        return await _context.Appointments
            .Include(a => a.Client)
            .Include(a => a.Therapist)
            .Include(a => a.ServiceType)
            .ToListAsync();
    }

    public async Task<IEnumerable<Appointment>> GetByClientAsync(Guid clientId)
    {
        return await _context.Appointments
            .Include(a => a.Client)
            .Include(a => a.Therapist)
            .Include(a => a.ServiceType)
            .Where(a => a.ClientId == clientId)
            .OrderByDescending(a => a.ScheduledAt)
            .ToListAsync();
    }

    public async Task<IEnumerable<Appointment>> GetByTherapistAsync(Guid therapistId)
    {
        return await _context.Appointments
            .Include(a => a.Client)
            .Include(a => a.Therapist)
            .Include(a => a.ServiceType)
            .Where(a => a.TherapistId == therapistId)
            .OrderByDescending(a => a.ScheduledAt)
            .ToListAsync();
    }

    public async Task<IEnumerable<Appointment>> GetByDateRangeAsync(DateTime start, DateTime end)
    {
        return await _context.Appointments
            .Include(a => a.Client)
            .Include(a => a.Therapist)
            .Include(a => a.ServiceType)
            .Where(a => a.ScheduledAt >= start && a.ScheduledAt < end)
            .OrderBy(a => a.ScheduledAt)
            .ToListAsync();
    }

    public async Task<IEnumerable<Appointment>> GetConflictingAsync(
        Guid therapistId, DateTime start, DateTime end)
    {
        return await _context.Appointments
            .Where(a => a.TherapistId == therapistId &&
                       a.Status != Domain.Enums.AppointmentStatus.Cancelled &&
                       a.ScheduledAt < end &&
                       a.ScheduledAt.AddMinutes(a.DurationMin) > start)
            .ToListAsync();
    }

    public async Task<Appointment> AddAsync(Appointment appointment)
    {
        await _context.Appointments.AddAsync(appointment);
        await _context.SaveChangesAsync();
        return appointment;
    }

    public async Task<Appointment> UpdateAsync(Appointment appointment)
    {
        _context.Appointments.Update(appointment);
        await _context.SaveChangesAsync();
        return appointment;
    }
}
