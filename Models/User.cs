using Microsoft.AspNetCore.Identity;

namespace NotatApp.Models
{
    public class User : IdentityUser
    {
            public ICollection<Note> Notes { get; set; } = new List<Note>();

    }
}