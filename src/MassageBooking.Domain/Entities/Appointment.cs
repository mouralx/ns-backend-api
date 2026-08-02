using MassageBooking.Domain.Enums;

namespace MassageBooking.Domain.Entities;

public class Appointment : BaseEntity
{
    public Guid ClientId { get; set; }
    public Guid TherapistId { get; set; }
    public Guid ServiceTypeId { get; set; }
    public DateTime ScheduledAt { get; set; }
    public int DurationMin { get; set; }
    public AppointmentStatus Status { get; set; } = AppointmentStatus.Scheduled;
    public ConfirmationStatus ConfirmationStatus { get; set; } = ConfirmationStatus.Pending;
    public DateTime? ConfirmedAt { get; set; }
    public DateTime? CancelledAt { get; set; }
    public string? CancellationReason { get; set; }
    public string? Notes { get; set; }
    public bool IsWalkin { get; set; }

    // Navigation properties
    public User Client { get; set; } = null!;
    public User Therapist { get; set; } = null!;
    public ServiceType ServiceType { get; set; } = null!;
    public ICollection<Notification> Notifications { get; set; } = new List<Notification>();
}