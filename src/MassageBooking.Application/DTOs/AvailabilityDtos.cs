using MassageBooking.Domain.Enums;

namespace MassageBooking.Application.DTOs;

public record AvailabilityRuleDto(
    Guid Id,
    int DayOfWeek,
    TimeOnly StartTime,
    TimeOnly EndTime,
    bool IsActive
);

public record AvailabilityBlockDto(
    Guid Id,
    DateTime StartAt,
    DateTime EndAt,
    string? Reason,
    bool IsRecurrence,
    string? RecurrenceRule
);

public record CreateRuleRequest(
    int DayOfWeek,
    TimeOnly StartTime,
    TimeOnly EndTime
);

public record UpdateRuleRequest(
    TimeOnly StartTime,
    TimeOnly EndTime,
    bool IsActive
);

public record CreateBlockRequest(
    DateTime StartAt,
    DateTime EndAt,
    string? Reason,
    bool IsRecurrence,
    string? RecurrenceRule
);

public record UpdateBlockRequest(
    DateTime StartAt,
    DateTime EndAt,
    string? Reason,
    bool IsRecurrence,
    string? RecurrenceRule
);
