namespace DocLink.Services;

public interface ISmsService
{
    Task SendAsync(string phoneNumber, string message);
}
