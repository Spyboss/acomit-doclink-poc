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

        // Step 1: Try sending as free-form text (works within 24h service window)
        if (await TrySendTextAsync(url, phoneNumber, message, accessToken))
            return;

        // Step 2: Text failed — Meta requires a template for business-initiated first message.
        LogTemplateSuggestion(phoneNumber);
        await SendTemplateFallbackAsync(url, phoneNumber, message, accessToken);
    }

    private async Task<bool> TrySendTextAsync(string url, string phoneNumber, string message, string accessToken)
    {
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

        if (response.IsSuccessStatusCode)
        {
            var body = await response.Content.ReadAsStringAsync();
            _logger.LogInformation("WhatsApp text sent to {PhoneNumber}. Response: {Response}", phoneNumber, body);
            return true;
        }

        var errorBody = await response.Content.ReadAsStringAsync();
        _logger.LogWarning("WhatsApp text failed for {PhoneNumber}. Status: {Status}. Error: {Error}",
            phoneNumber, response.StatusCode, errorBody);

        if (!IsTemplateRequiredError(errorBody))
        {
            throw new HttpRequestException($"WhatsApp API error: {response.StatusCode} - {errorBody}");
        }

        return false;
    }

    private async Task SendTemplateFallbackAsync(string url, string phoneNumber, string fallbackMessage, string accessToken)
    {
        var templateName = Environment.GetEnvironmentVariable("WHATSAPP_TEMPLATE_NAME") ?? "hello_world";

        object payload;

        if (templateName == "hello_world")
        {
            payload = new
            {
                messaging_product = "whatsapp",
                to = phoneNumber,
                type = "template",
                template = new
                {
                    name = "hello_world",
                    language = new { code = "en_US" }
                }
            };
        }
        else
        {
            payload = new
            {
                messaging_product = "whatsapp",
                to = phoneNumber,
                type = "template",
                template = new
                {
                    name = templateName,
                    language = new { code = "en" },
                    components = new[]
                    {
                        new
                        {
                            type = "body",
                            parameters = new[]
                            {
                                new { type = "text", text = fallbackMessage }
                            }
                        }
                    }
                }
            };
        }

        var json = JsonSerializer.Serialize(payload);
        var content = new StringContent(json, Encoding.UTF8, "application/json");

        _httpClient.DefaultRequestHeaders.Clear();
        _httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {accessToken}");

        var response = await _httpClient.PostAsync(url, content);
        var responseBody = await response.Content.ReadAsStringAsync();

        if (response.IsSuccessStatusCode)
        {
            _logger.LogInformation("WhatsApp template '{Template}' sent to {PhoneNumber}. Response: {Response}",
                templateName, phoneNumber, responseBody);
            return;
        }

        _logger.LogError("WhatsApp template '{Template}' also failed for {PhoneNumber}. Status: {Status}, Response: {Response}",
            templateName, phoneNumber, response.StatusCode, responseBody);

        _logger.LogWarning(
            "Sending requires either: (1) recipient has messaged your business number first (opens 24h window), " +
            "or (2) an approved template ('{Template}'). Set WHATSAPP_TEMPLATE_NAME env var to your approved template name.",
            templateName);

        throw new HttpRequestException($"WhatsApp API error: {response.StatusCode} - {responseBody}");
    }

    private void LogTemplateSuggestion(string phoneNumber)
    {
        _logger.LogWarning(
            "Free-form text to {PhoneNumber} was rejected — no open 24h service window. " +
            "Ask the recipient to message your business number first, or set WHATSAPP_TEMPLATE_NAME " +
            "to an approved template name.", phoneNumber);
    }

    private static bool IsTemplateRequiredError(string responseBody)
    {
        try
        {
            using var doc = JsonDocument.Parse(responseBody);
            if (!doc.RootElement.TryGetProperty("error", out var error))
                return false;

            if (error.TryGetProperty("code", out var code))
            {
                var codeVal = code.GetInt32();
                if (codeVal is 131000 or 131026 or 131008 or 131005)
                    return true;
            }

            if (error.TryGetProperty("error_data", out var data) &&
                data.TryGetProperty("details", out var details))
            {
                var detail = details.GetString() ?? "";
                if (detail.Contains("template", StringComparison.OrdinalIgnoreCase) ||
                    detail.Contains("not allowed", StringComparison.OrdinalIgnoreCase) ||
                    detail.Contains("service window", StringComparison.OrdinalIgnoreCase))
                    return true;
            }
        }
        catch { }

        return false;
    }
}
