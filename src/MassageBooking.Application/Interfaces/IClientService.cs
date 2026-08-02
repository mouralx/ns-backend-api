using MassageBooking.Application.DTOs;
using MassageBooking.Domain.Common;

namespace MassageBooking.Application.Interfaces;

public interface IClientService
{
    Task<Result<IEnumerable<ClientDto>>> ListAsync();
    Task<Result<ClientDetailDto>> GetDetailAsync(Guid clientId);
    Task<Result<IEnumerable<AppointmentDto>>> GetAppointmentsAsync(Guid clientId);
}
