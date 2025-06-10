using System.Security.Cryptography;

namespace Customers.Services;

public class PasswordHandler
{
    private static readonly int SaltSize = 16; // 128 bits
    private static readonly int HashSize = 32; // 256 bits
    private static readonly int Iterations = 10000;

    public static string GenerateSalt()
    {
        var salt = new byte[SaltSize];
        RandomNumberGenerator.Fill(salt);
        return Convert.ToBase64String(salt);
    }

    public static string HashPassword(string password, string salt)
    {
        using (var hmac = new Rfc2898DeriveBytes(
                   password,
                   Convert.FromBase64String(salt),
                   Iterations,
                   HashAlgorithmName.SHA256))
        {
            var hash = hmac.GetBytes(HashSize);
            return Convert.ToBase64String(hash);
        }
    }

    public static bool VerifyPassword(string password, string salt, string hash)
    {
        using (var hmac = new Rfc2898DeriveBytes(
                   password,
                   Convert.FromBase64String(salt),
                   Iterations,
                   HashAlgorithmName.SHA256))
        {
            var computedHash = hmac.GetBytes(HashSize);
            return Convert.ToBase64String(computedHash) == hash;
        }
    }
}