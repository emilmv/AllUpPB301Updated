using Microsoft.AspNetCore.Identity;

namespace AllUp.Models;

public class AppUser : IdentityUser
{
    public required string Fullname { get; set; }
    public bool IsBlocked { get; set; }
}
