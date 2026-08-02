namespace MassageBooking.Application.DTOs;

/// <summary>
/// Standard API response wrapper matching frontend expectations.
/// </summary>
public record ApiResponse<T>(T Data);

/// <summary>
/// Paginated API response wrapper matching frontend expectations.
/// </summary>
public record PaginatedResponse<T>(
    IEnumerable<T> Data,
    int Total,
    int Page,
    int PerPage);

/// <summary>
/// Empty response for operations that return no data.
/// </summary>
public record EmptyResponse();
