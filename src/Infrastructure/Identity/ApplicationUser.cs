using Microsoft.AspNetCore.Identity;

namespace RoomEnglish.Infrastructure.Identity;

public class ApplicationUser : IdentityUser
{
    /// <summary>
    /// Display name for the user (e.g., "Phòng Võ")
    /// This can contain special characters and spaces
    /// </summary>
    public string? DisplayName { get; set; }
}
