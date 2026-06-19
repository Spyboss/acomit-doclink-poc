using System.Text;
using System.Text.Json;

namespace DocLink.Services;

public class WhatsAppCloudApiService : IMessagingService
{
    private readonly ILogger<WhatsAppCloudApiService> _logger;
    private readonly HttpClient _httpClient;

    public WhatsAppCloudApiService(ILogger<WhatsAppCloudApiService> logger, HttpClient httpClient)
    {
        _logger = logger;
        _httpClient = httpClient;
    }

    public async Task SendAsync(string phoneNumber, string message)
    {
        var accessToken = Environment.GetEnvironmentVariable("WHATSAPP_ACCESS_TOKEN");
        var phoneNumberId = Environment.GetEnvironmentVariable("WHATSAPP_PHONE_NUMBER_ID");
        var apiVersion = Environment.GetEnvironmentVariable("WHATSAPP_API_VERSION") ?? "v22.0";

        if (string.IsNullOrEmpty(accessToken) || string.IsNullOrEmpty(phoneNumberId))
        {
            _logger.LogWarning("WhatsApp credentials not configured. Falling back to mock dispatch.");
            _logger.LogInformation("--- WhatsApp Message (not sent - no credentials) ---");
            _logger.LogInformation("To: {PhoneNumber}", phoneNumber);
            _logger.LogInformation("Message:\n{Message}", message);
            _logger.LogInformation("--------------------------------");
            return;
        }

        var url = $"https://graph.facebook.com/{apiVersion}/{phoneNumberId}/messages";

        var payload = new
        {
            messaging_product = "whatsapp",
            to = phoneNumber,
            type = "text",
            text = new { body = message }
        };

        var json = JsonSerializer.Serialize(payload);
        var content = new StringContent(json, Encoding.UTF8, "application/json");

        _httpClient.DefaultRequestHeaders.Clear();
        _httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {accessToken}");

        var response = await _httpClient.PostAsync(url, content);
        var responseBody = await response.Content.ReadAsStringAsync();

        if (response.IsSuccessStatusCode)
        {
            _logger.LogInformation("WhatsApp message sent successfully to {PhoneNumber}. Response: {Response}",
                phoneNumber, responseBody);
        }
        else
        {
            _logger.LogError("Failed to send WhatsApp message to {PhoneNumber}. Status: {Status}, Response: {Response}",
                phoneNumber, response.StatusCode, responseBody);
            throw new HttpRequestException($"WhatsApp API error: {response.StatusCode} - {responseBody}");
        }
    }
}
