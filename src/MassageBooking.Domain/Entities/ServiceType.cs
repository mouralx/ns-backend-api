namespace MassageBooking.Domain.Entities;

public class ServiceType : BaseEntity
{
    public string Name { get; set; } = string.Empty;
    public int DurationMin { get; set; }
    public string Description { get; set; } = string.Empty;
    public bool IsActive { get; set; } = true;

    // Navigation properties
    public ICollection<Appointment> Appointments { get; set; } = new List<Appointment>();
}
