using Microsoft.AspNetCore.Identity;
using UserProfile.Api.Models;
using UserProfile.Dal;

namespace UserProfile.Api.Extensions;

public static class Utils
{

    public static string CreatePwdHash(string password)
    {
        var hasher = new PasswordHasher<User>();
        return hasher.HashPassword(null, password);
    }

}