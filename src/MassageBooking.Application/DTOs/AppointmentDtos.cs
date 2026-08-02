using MassageBooking.Domain.Enums;

namespace MassageBooking.Application.DTOs;

public record AppointmentDto(
    Guid Id,
    Guid ClientId,
    UserDto Client,
    Guid TherapistId,
    UserDto Therapist,
    Guid ServiceTypeId,
    ServiceTypeDto ServiceType,
    DateTime ScheduledAt,
    int DurationMin,
    AppointmentStatus Status,
    ConfirmationStatus ConfirmationStatus,
    DateTime? ConfirmedAt,
    DateTime? CancelledAt,
    string? CancellationReason,
    string? Notes,
    bool IsWalkin,
    DateTime CreatedAt
);

public record BookAppointmentRequest(
    Guid TherapistId,
    Guid ServiceTypeId,
    DateTime ScheduledAt,
    string? Notes
);

public record UpdateAppointmentRequest(
    DateTime? ScheduledAt,
    string? Notes
);

public record SlotDto(
    DateTime StartAt,
    DateTime EndAt
);

public record SlotsResponse(
    DateTime Date,
    Guid TherapistId,
    Guid ServiceTypeId,
    int DurationMin,
    IEnumerable<SlotDto> Slots
);
