namespace MassageBooking.Application.Interfaces;

public interface INotificationJobService
{
    Task Send24HourConfirmationsAsync();
    Task Send2HourRemindersAsync();
    Task DetectNoShowsAsync();
}
