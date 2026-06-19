namespace DocLink.Services;

public class MockMessagingService : IMessagingService
{
    private readonly ILogger<MockMessagingService> _logger;

    public MockMessagingService(ILogger<MockMessagingService> logger)
    {
        _logger = logger;
    }

    public Task SendAsync(string phoneNumber, string message)
    {
        _logger.LogInformation("--- WhatsApp Message Dispatch ---");
        _logger.LogInformation("To: {PhoneNumber}", phoneNumber);
        _logger.LogInformation("Message:\n{Message}", message);
        _logger.LogInformation("--------------------------------");

        return Task.CompletedTask;
    }
}
