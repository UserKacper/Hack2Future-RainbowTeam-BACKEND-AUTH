using Microsoft.AspNetCore.Mvc;

namespace FraudDetection.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ClaimsController : ControllerBase
    {
        [HttpPost("create-insurance-claim")]
        public async Task<ActionResult> CreateClaim([FromBody] string claim)
        {
            // Here you would typically add logic to create a claim in your database
            // For demonstration purposes, we will just return the claim as a response

            if (string.IsNullOrEmpty(claim))
            {
                return BadRequest("Claim cannot be null or empty.");
            }

            // Simulate claim creation logic
            await Task.Delay(100); // Simulating async operation

            return Ok(new { Message = "Claim created successfully", Claim = claim });
        }
    }
}
