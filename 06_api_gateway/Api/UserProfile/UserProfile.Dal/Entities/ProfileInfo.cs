using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Users.Dal.Entities;

public class ProfileInfo
{
    [Key]
    public int Id { get; set; }

    public string FirstName { get; set; }
    
    public string LastName { get; set; }

    public string? Phone { get; set; }

    public string? AvatarUri { get; set; }

    public int? Age { get; set; }

    // Foreign key property
    [ForeignKey("User")]
    public int? UserId { get; set; }

    // Navigation property
    public User User { get; set; }
}