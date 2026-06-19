using System.Security.Cryptography;

namespace DocLink.Services;

public class TokenService : ITokenService
{
    private const string Alphabet = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789";

    public string GenerateToken(int length = 10)
    {
        return RandomNumberGenerator.GetString(Alphabet, length);
    }
}
