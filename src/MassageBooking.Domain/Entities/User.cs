using MassageBooking.Domain.Enums;

namespace MassageBooking.Domain.Entities;

public class User : BaseEntity
{
    public string Email { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public UserRole Role { get; set; }
    public string PasswordHash { get; set; } = string.Empty;
    public string? PushToken { get; set; }
    public bool IsActive { get; set; } = true;

    // Navigation properties
    public ICollection<Appointment> ClientAppointments { get; set; } = new List<Appointment>();
    public ICollection<Appointment> TherapistAppointments { get; set; } = new List<Appointment>();
    public ICollection<AvailabilityRule> AvailabilityRules { get; set; } = new List<AvailabilityRule>();
    public ICollection<AvailabilityBlock> AvailabilityBlocks { get; set; } = new List<AvailabilityBlock>();
    public ICollection<Notification> Notifications { get; set; } = new List<Notification>();
}
