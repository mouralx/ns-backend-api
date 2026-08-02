namespace MassageBooking.Application.DTOs;

public record TodayScheduleDto(
    List<AppointmentDto> Appointments);

public record DashboardStatsDto(
    int TotalBooked,
    int Confirmed,
    int Cancelled,
    int NoShow,
    double ConfirmationRate);

public record AtRiskAppointmentDto(
    Guid Id,
    string ClientName,
    string ServiceName,
    DateTime ScheduledAt,
    DateTime ConfirmationDeadline);
