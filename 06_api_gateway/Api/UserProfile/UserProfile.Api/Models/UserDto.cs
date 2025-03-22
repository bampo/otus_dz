#nullable disable

using System.ComponentModel.DataAnnotations;

namespace UserProfile.Api.Models;

public class UserDto
{
    [Required(ErrorMessage = "Имя пользователя обязательно.")]
    [StringLength(50, ErrorMessage = "Имя пользователя должно быть от 3 до 50 символов.", MinimumLength = 3)]
    public string Username { get; set; }

    [Required(ErrorMessage = "Имя обязательно.")]
    [StringLength(100, ErrorMessage = "Имя должно быть от 2 до 100 символов.", MinimumLength = 2)]
    public string FirstName { get; set; }

    [Required(ErrorMessage = "Фамилия обязательна.")]
    [StringLength(100, ErrorMessage = "Фамилия должна быть от 2 до 100 символов.", MinimumLength = 2)]
    public string LastName { get; set; }

    [Required(ErrorMessage = "Email обязателен.")]
    [EmailAddress(ErrorMessage = "Неверный формат Email.")]
    public string Email { get; set; }

    [Phone(ErrorMessage = "Неверный номер телефона.")]
    public string Phone { get; set; }
}