using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Diagnostics.CodeAnalysis;
using System.Text.Json.Serialization;

namespace FraudDetection.Database.Models
{
    public class InsuranceClaim
    {
        [Key]
        public string Id { get; set; }

        // Foreign key to user
        [Required]
        public string AppUserId{ get; set; }
        [JsonIgnore]
        public AppUser AppUser { get; set; }

        // Claim details
        [Required]
        [MaxLength(100)]
        public string ClaimType { get; set; } // e.g., Collision, Theft, PIP, Application, Repair, Agent

        [Required]
        [MaxLength(100)]
        public string FraudSubtype { get; set; } // e.g., Swoop and Squat, Owner Give-Up, Exaggerated Injuries

        [MaxLength(1000)]
        public string Description { get; set; }
        [Url]
        [AllowNull]
        public string ImageUrl { get; set; }
        public DateTime DateOfClaim { get; set; }

        [MaxLength(50)]
        public string ClaimStatus { get; set; } // e.g., Pending, Approved, Denied, Under Investigation
        public bool IsPotentialFraud { get; set; } = false;

        public bool IsConfirmedFraud { get; set; } = false;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public DateTime? UpdatedAt { get; set; }
    }
}
