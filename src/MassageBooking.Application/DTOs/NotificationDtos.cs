using MassageBooking.Domain.Enums;

namespace MassageBooking.Application.DTOs;

public record NotificationDto(
    Guid Id,
    NotificationType Type,
    NotificationChannel Channel,
    NotificationStatus Status,
    DateTime? SentAt,
    DateTime? ReadAt,
    Guid AppointmentId,
    string Message);

public record MarkReadRequest(
    List<Guid> NotificationIds);
