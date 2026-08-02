namespace MassageBooking.Application.DTOs;

public record AvailabilityRuleDto(
    Guid Id,
    int DayOfWeek,
    TimeOnly StartTime,
    TimeOnly EndTime,
    bool IsActive);

public record CreateRuleRequest(
    int DayOfWeek,
    TimeOnly StartTime,
    TimeOnly EndTime);

public record AvailabilityBlockDto(
    Guid Id,
    DateTime StartAt,
    DateTime EndAt,
    string? Reason,
    bool IsRecurrence,
    string? RecurrenceRule);

public record CreateBlockRequest(
    DateTime StartAt,
    DateTime EndAt,
    string? Reason,
    bool IsRecurrence = false,
    string? RecurrenceRule = null);
