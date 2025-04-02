#nullable disable

using System.ComponentModel.DataAnnotations;

namespace UserProfile.Api.Models;

public class RegisterUserDto
{
    [Required]
    public string Email { get; set; }

    [Required]
    public string Password { get; set; }
}

public class UpdateUserDto
{
    public string Password { get; set; }
}