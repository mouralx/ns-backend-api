namespace MassageBooking.Domain.Entities;

public class AvailabilityRule : BaseEntity
{
    public Guid TherapistId { get; set; }
    public int DayOfWeek { get; set; } // 0-6 (Sunday-Saturday)
    public TimeOnly StartTime { get; set; }
    public TimeOnly EndTime { get; set; }
    public bool IsActive { get; set; } = true;

    // Navigation property
    public User Therapist { get; set; } = null!;
}
