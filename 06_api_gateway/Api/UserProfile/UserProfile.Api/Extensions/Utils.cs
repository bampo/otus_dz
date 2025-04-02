using Microsoft.AspNetCore.Identity;
using Users.Dal.Entities;

namespace UserProfile.Api.Extensions;

public static class Utils
{

    public static string CreatePwdHash(string password)
    {
        var hasher = new PasswordHasher<User>();
        return hasher.HashPassword(null, password);
    }

}