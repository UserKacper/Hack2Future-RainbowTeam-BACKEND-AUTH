using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FraudDetection.Database.Models
{
    public class InsuranceClaim
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public Guid Id { get; set; }

        // Foreign key to user
        [Required]
        public Guid UserId { get; set; }
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

        public DateTime DateOfClaim { get; set; }

        [MaxLength(50)]
        public string ClaimStatus { get; set; } // e.g., Pending, Approved, Denied, Under Investigation

        public bool IsPotentialFraud { get; set; } = false;

        public bool IsConfirmedFraud { get; set; } = false;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public DateTime? UpdatedAt { get; set; }
    }
}
