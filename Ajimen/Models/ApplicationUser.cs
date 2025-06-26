using Microsoft.AspNetCore.Identity;

namespace Ajimen.Models
{
    public class ApplicationUser : IdentityUser
    {      
        public string UserFullName { get; set; }
        public string UserRole { get; set; }
    }
}