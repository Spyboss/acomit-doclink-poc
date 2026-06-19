namespace DocLink.Services;

public interface ITokenService
{
    string GenerateToken(int length = 10);
}
