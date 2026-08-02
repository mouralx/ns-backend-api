using MassageBooking.Application.DTOs;
using MassageBooking.Domain.Common;

namespace MassageBooking.Application.Interfaces;

public interface IDashboardService
{
    Task<Result<TodayScheduleDto>> GetTodayScheduleAsync(Guid therapistId);
    Task<Result<DashboardStatsDto>> GetStatsAsync(Guid therapistId);
    Task<Result<List<AtRiskAppointmentDto>>> GetAtRiskAppointmentsAsync(Guid therapistId);
}
