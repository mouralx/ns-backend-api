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
    DateTime CreatedAt);

public record BookAppointmentRequest(
    Guid ServiceTypeId,
    Guid TherapistId,
    DateTime ScheduledAt,
    string? Notes = null);

public record UpdateAppointmentRequest(
    DateTime? ScheduledAt,
    int? DurationMin,
    string? Notes);

public record SlotDto(
    DateTime Start,
    DateTime End);

public record SlotsResponse(
    DateTime Date,
    Guid TherapistId,
    Guid ServiceTypeId,
    int DurationMin,
    List<SlotDto> Slots);
