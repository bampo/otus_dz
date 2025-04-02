namespace UserProfile.Api.Models;

public class ProfileDto
{
    public int UserId { get; set; }

    public string FirstName { get; set; }

    public string LastName { get; set; }

    public string? Phone { get; set; }

    public string? AvatarUri { get; set; }

    public int? Age { get; set; }
}