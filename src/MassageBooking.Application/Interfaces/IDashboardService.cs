using MassageBooking.Application.DTOs;
using MassageBooking.Domain.Common;

namespace MassageBooking.Application.Interfaces;

public interface IDashboardService
{
    Task<Result<DashboardStatsDto>> GetStatsAsync();
    Task<Result<IEnumerable<AppointmentDto>>> GetTodayScheduleAsync();
    Task<Result<IEnumerable<AppointmentDto>>> GetUpcomingAsync(int limit = 10);
    Task<Result<IEnumerable<AppointmentDto>>> GetAtRiskAppointmentsAsync();
}
