using FraudDetection.Database;
using FraudDetection.Database.Models;
using FraudDetection.DTOs;
using FraudDetection.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace FraudDetection.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ClaimsController : ControllerBase
    {
        private readonly ILogger<ClaimsController> _logger;
        private readonly DatabaseContext _context;
        private readonly UserManager<AppUser> _userManager;
        private readonly IBlobStorageService _blobStorageService;

        public ClaimsController(ILogger<ClaimsController> logger, DatabaseContext context, UserManager<AppUser> userManager, IBlobStorageService blobStorageService)
        {
            _logger = logger;
            _context = context;
            _userManager = userManager;
            _blobStorageService = blobStorageService;
        }

        [HttpGet("get-all")]
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
        public async Task<ActionResult<IEnumerable<InsuranceClaim>>> GetUserClaims([FromRoute] string id)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            try
            {
                var user = await _userManager.FindByIdAsync(id);
                if (user == null)
                {
                    return NotFound("User not found");
                }

                var claims = await _context.InsuranceClaims
                    .Where(c => c.AppUserId == id)
                    .ToListAsync();

                return Ok(claims);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving user claims");
                return StatusCode(500, "Internal server error");
            }
        }

        [HttpPost("upload-claim-image")]
        public async Task<ActionResult> UploadClaimImage([FromForm] IFormFile file, [FromQuery] string claimId)
        {
            if (file == null || file.Length == 0)
            {
                return BadRequest("No file uploaded");
            }

            if (string.IsNullOrEmpty(claimId))
            {
                return BadRequest("Claim ID is required");
            }

            try
            {
                var claim = await _context.InsuranceClaims.FindAsync(claimId);
                if (claim == null)
                {
                    return NotFound("Claim not found");
                }

                using var stream = file.OpenReadStream();
                var imageUrl = await _blobStorageService.UploadFileAsync(
                    stream,
                    file.FileName,
                    file.ContentType);

                claim.ImageUrl = imageUrl;
                await _context.SaveChangesAsync();

                return Ok(new { ImageUrl = claim.ImageUrl });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error uploading claim image");
                return StatusCode(500, "Internal server error");
            }
        }

        [HttpPost("create")]
        public async Task<ActionResult> CreateClaim([FromBody] CreateInsuranceClaimDto claim, [FromQuery] string userId)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            if (string.IsNullOrEmpty(userId))
            {
                return BadRequest("User ID is required");
            }

            try
            {
                var user = await _userManager.FindByIdAsync(userId);
                if (user == null)
                {
                    return NotFound($"User with ID {userId} not found");
                }

                if (string.IsNullOrEmpty(claim.ClaimType) || string.IsNullOrEmpty(claim.FraudSubtype))
                {
                    return BadRequest("ClaimType and FraudSubtype are required fields");
                }

                var insuranceClaim = new InsuranceClaim
                {
                    Id = Guid.NewGuid().ToString(),
                    ClaimStatus = claim.ClaimStatus ?? "Pending",
                    ClaimType = claim.ClaimType.Trim(),
                    Description = claim.Description?.Trim() ?? string.Empty,
                    CreatedAt = DateTime.UtcNow,
                    AppUserId = userId,  // Add this line to fix the not-null constraint
                    FraudSubtype = claim.FraudSubtype.Trim(),
                    DateOfClaim = claim.DateOfClaim == default ? DateTime.UtcNow : claim.DateOfClaim,
                    IsPotentialFraud = claim.IsPotentialFraud,
                    IsConfirmedFraud = claim.IsConfirmedFraud,
                    ImageUrl = claim.ImageUrl ?? string.Empty
                };

                await _context.InsuranceClaims.AddAsync(insuranceClaim);
                
                try
                {
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateException dbEx)
                {
                    _logger.LogError(dbEx, "Database error while saving insurance claim");
                    return StatusCode(500, "Failed to save the insurance claim");
                }

                return CreatedAtAction(
                    nameof(GetUserClaims),
                    new { id = userId },
                    insuranceClaim);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error while creating insurance claim for user {UserId}", userId);
                return StatusCode(500, "An unexpected error occurred while processing your request");
            }
        }
    }
}
