using Microsoft.AspNetCore.Identity;

namespace WorkChat2.Models
{
    public class AppUser : IdentityUser
    {
        public required String Name { get; set; }
        public required String LastName { get; set; }

        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
    }
}
