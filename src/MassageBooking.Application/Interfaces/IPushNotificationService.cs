namespace MassageBooking.Application.Interfaces;

public interface IPushNotificationService
{
    Task<bool> SendPushAsync(string token, string title, string body, Dictionary<string, string>? data = null);
}
