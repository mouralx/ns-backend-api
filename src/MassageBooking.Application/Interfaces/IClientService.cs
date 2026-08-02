using MassageBooking.Application.DTOs;
using MassageBooking.Domain.Common;

namespace MassageBooking.Application.Interfaces;

public interface IClientService
{
    Task<Result<IEnumerable<ClientDto>>> GetAllAsync();
    Task<Result<ClientDto>> GetByIdAsync(Guid id);
    Task<Result<ClientDto>> UpdateAsync(Guid id, UpdateClientRequest request);
    Task<Result<IEnumerable<AppointmentDto>>> GetAppointmentHistoryAsync(Guid clientId);
}
