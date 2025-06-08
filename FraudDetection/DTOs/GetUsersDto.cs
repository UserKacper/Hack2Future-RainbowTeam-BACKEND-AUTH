
namespace FraudDetection.DTOs
{
    public class GetUsersDto
    {
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string UniqueIdNumber { get; set; }
        public string Email { get; set; }
        public string Id { get; set; }
        public string Role { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public List<GetInsuranceClaimsDto> InsuranceClaims { get; set; } = new List<GetInsuranceClaimsDto>();
        public List<string> InsuranceClaimsIds { get; set; } = new List<string>();
    }
}
