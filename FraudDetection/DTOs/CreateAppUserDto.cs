namespace FraudDetection.DTOs
{
    public class CreateAppUserDto
    {
        public required string Email { get; set; }
        public required string Password { get; set; }
        public required string FirstName { get; set; }
        public required string LastName { get; set; }
        public required string UniqueIdNumber {  get; set; }
    }
}
