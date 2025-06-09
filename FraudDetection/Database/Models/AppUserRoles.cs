using Microsoft.AspNetCore.Identity;

namespace FraudDetection.Database.Models
{
    public class AppUserRoles:IdentityRole<string>
    {
        public AppUserRoles()
        {
            Id = Guid.NewGuid().ToString(); 
        }
    }
}
