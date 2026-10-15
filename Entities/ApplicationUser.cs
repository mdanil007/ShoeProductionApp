using Microsoft.AspNetCore.Identity;

namespace ShoeProductionApp.Entities
{
    public class ApplicationUser : IdentityUser
    {
        public bool IsActive { get; set; } = true;
    }
}
