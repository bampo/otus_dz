using Auth.Dal;
using Microsoft.AspNetCore.Identity;

namespace Auth.Api;

public static class Utils
{
    public static bool ValidatePassword(string hash, string password)
    {
        var hasher = new PasswordHasher<User>();
        return hasher.VerifyHashedPassword(null, hash, password) != PasswordVerificationResult.Failed;
    }
}