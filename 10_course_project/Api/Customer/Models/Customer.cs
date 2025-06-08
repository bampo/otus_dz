#nullable disable
using System.ComponentModel.DataAnnotations;

namespace Customers.Models;

public class Customer
{
    [Key]
    public Guid Id { get; set; }

    [Required]
    [MaxLength(100)]
    public string FirstName { get; set; }

    [Required]
    [MaxLength(100)]
    public string LastName { get; set; }

    [Required]
    [EmailAddress]
    [MaxLength(256)]
    public string Email { get; set; }

    [Required]
    [DataType(DataType.Password)]
    public string PasswordHash { get; set; }

    [Required]
    public string Salt { get; set; }
}