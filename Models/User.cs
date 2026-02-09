using Microsoft.AspNetCore.Identity;

namespace NotatApp.Models
{
    public class User : IdentityUser
    {
        public string? RefreshToken { get; set; }
        public DateTime? RefreshTokenExpiresAt { get; set; }

    }
}