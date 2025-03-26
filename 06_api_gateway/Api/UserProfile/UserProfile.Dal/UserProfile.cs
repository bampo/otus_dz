using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace UserProfile.Dal;

public class UserProfile
{
    [Key]
    public int Id { get; set; }

    public string? Phone { get; set; }

    public string? AvatarUri { get; set; }

    public int? Age { get; set; }

    // Foreign key property
    [ForeignKey("User")]
    public int? UserId { get; set; }

    // Navigation property
    public User User { get; set; }
}