namespace MassageBooking.Domain.Enums;

public enum NotificationType
{
    BookingCreated = 0,
    BookingConfirmation = 1,
    AppointmentReminder = 2,
    AppointmentCancelled = 3,
    AppointmentConfirmed = 4,
    AppointmentUpdate = 5,
    ConfirmationRequest = 6,
    General = 7
}
