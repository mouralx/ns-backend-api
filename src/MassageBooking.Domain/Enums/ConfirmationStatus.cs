namespace MassageBooking.Domain.Enums;

public enum ConfirmationStatus
{
    Unconfirmed = 0,
    PendingConfirmation = 1,
    Confirmed = 2,
    Expired = 3,
    AtRisk = 4,
    Rejected = 5
}
