namespace MassageBooking.Domain.Entities;

public class AvailabilityBlock : BaseEntity
{
    public Guid TherapistId { get; set; }
    public DateTime StartAt { get; set; }
    public DateTime EndAt { get; set; }
    public string? Reason { get; set; }
    public bool IsRecurrence { get; set; }
    public string? RecurrenceRule { get; set; }

    // Navigation property
    public User Therapist { get; set; } = null!;
}
