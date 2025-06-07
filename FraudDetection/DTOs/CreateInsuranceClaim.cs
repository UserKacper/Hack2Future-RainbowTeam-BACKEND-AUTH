using System.ComponentModel.DataAnnotations;

namespace FraudDetection.DTOs
{
    public class CreateInsuranceClaim
    {
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
        public string ImageUrl { get; set; } // Optional field for image URL associated with the claim
        public string VideoUrl { get; set; } // Optional field for video URL associated with the claim
    }
}
