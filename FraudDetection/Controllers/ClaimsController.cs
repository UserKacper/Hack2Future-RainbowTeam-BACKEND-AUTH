using FraudDetection.Database;
using FraudDetection.Database.Models;
using FraudDetection.DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace FraudDetection.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ClaimsController : ControllerBase
    {
        private readonly ILogger<ClaimsController> _logger;
        private readonly DatabaseContext _context;
        private readonly UserManager<AppUser> _userManager;
        public ClaimsController (ILogger<ClaimsController> logger, DatabaseContext context, UserManager<AppUser> userManager)
        {
            _logger = logger;
            _context = context;
            _userManager = userManager;
        }

        [HttpGet("get-all-claims")]
        public async Task<ActionResult<IEnumerable<InsuranceClaim>>> GetAllClaims()
        {
            try
            {
                var claims = await _context.InsuranceClaims.ToListAsync();
                return Ok(claims);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving insurance claims");
                return StatusCode(500, "Internal server error");
            }
        }
        [HttpGet("get-user-claims/{id}")]
        public async Task<ActionResult<IEnumerable<InsuranceClaim>>> GetUserClaims([FromRoute]Guid id)
        {
            if(!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            try
            {
                var user = await _userManager.FindByIdAsync(id.ToString());
                if (user == null)
                {
                    return NotFound("User not found");
                }

                var claims = await _context.InsuranceClaims
                    .Where(c => c.UserId == id)
                    .ToListAsync();

                return Ok(claims);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving user claims");
                return StatusCode(500, "Internal server error");
            }
        }

        [HttpPost("create-insurance-claim")]
        public async Task<ActionResult> CreateClaim ([FromBody] CreateInsuranceClaimDto claim)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            try
            {
                Guid UserId = Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value);

                var insuranceClaim = new InsuranceClaim
                {
                    ClaimStatus = claim.ClaimStatus,
                    ClaimType = claim.ClaimType,
                    Description = claim.Description,
                    CreatedAt = DateTime.UtcNow,
                    UserId = UserId,
                    FraudSubtype = claim.FraudSubtype,
                    DateOfClaim = claim.DateOfClaim,
                    IsPotentialFraud = claim.IsPotentialFraud,
                    IsConfirmedFraud = claim.IsConfirmedFraud
                };

                _context.InsuranceClaims.Add(insuranceClaim); 

                await _context.SaveChangesAsync();

                return CreatedAtAction(nameof(CreateClaim), new { id = insuranceClaim.Id }, insuranceClaim);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating insurance claim");
                return StatusCode(500, "Internal server error");
            }
        }
    }
}
