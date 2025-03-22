using UserProfile.Api.Models;
using UserProfile.Dal;

namespace UserProfile.Api.Extensions;

public static class UserDtoExtensions
{
    public static void CopyUser(this UserDto userDto, User user)
    {
        user.Username = userDto.Username;
        user.FirstName = userDto.FirstName;
        user.LastName = userDto.LastName;
        user.Email = userDto.Email;
        user.Phone = userDto.Phone;
    }
}