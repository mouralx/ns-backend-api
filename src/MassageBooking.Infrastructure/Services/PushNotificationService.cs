using System.Text;
using System.Text.Json;
using MassageBooking.Application.Interfaces;

namespace MassageBooking.Infrastructure.Services;

public class PushNotificationService : IPushNotificationService
{
    private readonly HttpClient _httpClient;

    public PushNotificationService(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<bool> SendPushAsync(string token, string title, string body, Dictionary<string, string>? data = null)
    {
        try
        {
            var payload = new
            {
                to = token,
                title = title,
                body = body,
                data = data ?? new Dictionary<string, string>()
            };

            var json = JsonSerializer.Serialize(payload);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await _httpClient.PostAsync("https://exp.host/--/api/v2/push/send", content);

            return response.IsSuccessStatusCode;
        }
        catch
        {
            return false;
        }
    }
}
