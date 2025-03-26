#nullable disable
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace UserProfile.Dal;

public class User
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int? Id { get; init; }

    [Required]
    public string Email { get; set; }
    
    [Required]
    public string PasswordHash { get; set; }

    public string FirstName { get; set; }
    
    public string LastName { get; set; }

    public ICollection<UserProfile> Profile { get; set; }
}