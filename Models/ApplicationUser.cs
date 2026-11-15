
using Microsoft.AspNetCore.Identity;

namespace CampusKonnect.Models
{
    public class ApplicationUser : IdentityUser
    {
        public string FullName { get; set; } = string.Empty;
    }
}
