namespace DocLink.Services;

public interface IMessagingService
{
    Task SendAsync(string phoneNumber, string message);
    string Channel => "WhatsApp";
}
