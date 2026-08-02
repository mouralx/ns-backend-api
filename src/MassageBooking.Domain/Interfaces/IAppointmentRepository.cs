using MassageBooking.Domain.Entities;

namespace MassageBooking.Domain.Interfaces;

public interface IAppointmentRepository
{
    Task<Appointment?> GetByIdAsync(Guid id);
    Task<IEnumerable<Appointment>> GetAllAsync();
    Task<IEnumerable<Appointment>> GetByClientAsync(Guid clientId);
    Task<IEnumerable<Appointment>> GetByTherapistAsync(Guid therapistId);
    Task<IEnumerable<Appointment>> GetByDateRangeAsync(DateTime start, DateTime end);
    Task<IEnumerable<Appointment>> GetConflictingAsync(Guid therapistId, DateTime start, DateTime end);
    Task<Appointment> AddAsync(Appointment appointment);
    Task<Appointment> UpdateAsync(Appointment appointment);
}
