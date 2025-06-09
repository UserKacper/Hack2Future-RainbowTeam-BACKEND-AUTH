using Microsoft.AspNetCore.Identity;

namespace FraudDetection.Database.Models
{
    public class AppUser : IdentityUser<string>
    {
        public AppUser()
        {
            Id = Guid.NewGuid().ToString();
            CreatedAt = DateTime.UtcNow;
        }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public List<InsuranceClaim> InsuranceClaims { get; set; } = new List<InsuranceClaim>();
    }
}
