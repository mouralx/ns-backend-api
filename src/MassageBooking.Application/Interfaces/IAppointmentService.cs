using MassageBooking.Application.DTOs;
using MassageBooking.Domain.Common;

namespace MassageBooking.Application.Interfaces;

public interface IAppointmentService
{
    Task<Result<AppointmentDto>> GetByIdAsync(Guid id);
    Task<Result<IEnumerable<AppointmentDto>>> GetAllAsync();
    Task<Result<IEnumerable<AppointmentDto>>> GetByClientAsync(Guid clientId);
    Task<Result<IEnumerable<AppointmentDto>>> GetByTherapistAsync(Guid therapistId);
    Task<Result<AppointmentDto>> BookAsync(BookAppointmentRequest request, Guid clientId);
    Task<Result<AppointmentDto>> CancelAsync(Guid id, string? reason = null);
    Task<Result<AppointmentDto>> ConfirmAsync(Guid id);
    Task<Result<SlotsResponse>> GetAvailableSlotsAsync(Guid therapistId, Guid serviceTypeId, DateTime date);
}
