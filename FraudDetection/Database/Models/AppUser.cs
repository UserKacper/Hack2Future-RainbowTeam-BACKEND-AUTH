using Microsoft.AspNetCore.Identity;

namespace FraudDetection.Database.Models
{
    public class AppUser : IdentityUser<Guid>
    {
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string UniqueIdNumber {  get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public List<InsuranceClaim> InsuranceClaims { get; set; } = new List<InsuranceClaim>();
    }
}
