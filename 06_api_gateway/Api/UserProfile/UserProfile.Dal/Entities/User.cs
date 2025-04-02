#nullable disable
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Users.Dal.Entities;

public class User
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int? Id { get; init; }

    [Required]
    public string Email { get; set; }
    
    [Required]
    public string PasswordHash { get; set; }

    public DateTime CreatedAt { get; set; }


    public ProfileInfo ProfileInfo { get; set; }
}