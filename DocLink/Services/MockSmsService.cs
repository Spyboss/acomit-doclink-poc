namespace DocLink.Services;

public class MockSmsService : ISmsService
{
    private readonly ILogger<MockSmsService> _logger;

    public MockSmsService(ILogger<MockSmsService> logger)
    {
        _logger = logger;
    }

    public Task SendAsync(string phoneNumber, string message)
    {
        _logger.LogInformation("--- SMS Dispatch ---");
        _logger.LogInformation("To: {PhoneNumber}", phoneNumber);
        _logger.LogInformation("Message:\n{Message}", message);
        _logger.LogInformation("--------------------");

        return Task.CompletedTask;
    }
}
