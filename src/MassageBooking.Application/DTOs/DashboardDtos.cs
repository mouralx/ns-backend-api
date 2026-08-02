using MassageBooking.Domain.Enums;

namespace MassageBooking.Application.DTOs;

public record DashboardStatsDto(
    int TodayAppointments,
    int TotalClients,
    int TotalTherapists,
    int TotalServices,
    int PendingAppointments,
    int ConfirmedAppointments
);
