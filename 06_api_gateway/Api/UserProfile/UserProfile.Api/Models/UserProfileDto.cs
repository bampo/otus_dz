namespace UserProfile.Api.Models;

public class UserProfileDto
{
    public int UserId { get; set; }
    public string? AvatarUri { get; set; }
    protected int? Age { get; set; }
}