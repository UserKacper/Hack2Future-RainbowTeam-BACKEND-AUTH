using FraudDetection.Contracts;
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
    public class AccountsController : ControllerBase
    {
        private readonly ILogger<AccountsController> _logger;
        private readonly DatabaseContext _context;
        private readonly UserManager<AppUser> _userManager;
        private readonly RoleManager<AppUserRoles> _roleManager;
        private readonly ITokenService _tokenService;

        public AccountsController (ILogger<AccountsController> logger, DatabaseContext context, UserManager<AppUser> userManager, RoleManager<AppUserRoles> roleManager, ITokenService tokenService)
        {
            _logger = logger;
            _context = context;
            _userManager = userManager;
            _roleManager = roleManager;
            _tokenService = tokenService;
        }
        [HttpPost("login")]
        public async Task<IActionResult> Login ([FromBody] LoginContract login)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            try
            {
                var user = await _userManager.FindByEmailAsync(login.Email);
                if (user == null)
                {
                    return Unauthorized("Invalid email or password.");
                }

                var result = await _userManager.CheckPasswordAsync(user, login.Password);
                if (!result)
                {
                    return Unauthorized("Invalid email or password.");
                }

                var token = await _tokenService.CreateJWT(user);

                return Ok(new { Token = token });

            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while logging in.");
                return StatusCode(500, "Internal server error");
            }
        }


        [HttpPost("verify-email")]
        public async Task<IActionResult> VerifyEmail ([FromQuery] string userId, [FromQuery] string token)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            try
            {
                var user = await _userManager.FindByIdAsync(userId);

                if (user == null)
                {
                    return NotFound("User not found.");
                }

                var confirmEmail = await _userManager.ConfirmEmailAsync(user, token);

                if (!confirmEmail.Succeeded)
                {
                    return BadRequest("Invalid token");
                }

                return Ok("Email verified successfully.");

            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while verifying email.");
                return StatusCode(500, "Internal server error");
            }
        }

        [HttpPost("send-email-confirmation")]
        public async Task<IActionResult> SendEmailConfirmation ([FromQuery] string userId)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            try
            {
                var user = await _userManager.FindByIdAsync(userId);
                if (user == null)
                {
                    return NotFound("User not found.");
                }
                var token = await _userManager.GenerateEmailConfirmationTokenAsync(user);

                return Ok(new { Token = token });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while sending email confirmation.");
                return StatusCode(500, "Internal server error");
            }
        }

        [HttpGet("get-all")]
        public async Task<ActionResult<IEnumerable<GetUsersDto>>> GetUsers ()
        {
            try
            {
                var usersInUserRole = await _userManager.GetUsersInRoleAsync("User");

                if (!usersInUserRole.Any())
                    return Ok(new List<GetUsersDto>());

                var userDtos = usersInUserRole.Select(user => new GetUsersDto
                {
                    Id = user.Id,
                    Email = user.Email ?? string.Empty,
                    FirstName = user.FirstName ?? string.Empty,
                    LastName = user.LastName ?? string.Empty,
                    CreatedAt = user.CreatedAt,
                    UpdatedAt = user.UpdatedAt,
                    InsuranceClaims = user.InsuranceClaims.Select(claim => new GetInsuranceClaimsDto
                    {
                        Id = claim.Id,
                        ClaimType = claim.ClaimType,
                        FraudSubtype = claim.FraudSubtype,
                        Description = claim.Description,
                        DateOfClaim = claim.DateOfClaim,
                        ClaimStatus = claim.ClaimStatus,
                        IsPotentialFraud = claim.IsPotentialFraud,
                        IsConfirmedFraud = claim.IsConfirmedFraud
                    }).ToList(),
                    Role = "User",
                }).ToList();

                return Ok(userDtos);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while retrieving users.");
                return StatusCode(500, "Internal server error");
            }
        }




        [HttpGet("get/{id}")]
        public async Task<ActionResult<GetUsersDto>> GetUserById (Guid id)
        {
            try
            {
                var user = await _userManager.FindByIdAsync(id.ToString());
                if (user == null)
                {
                    return NotFound("User not found.");
                }

                var userDto = new GetUsersDto
                {
                    Id = user.Id,
                    Email = user.Email,
                    FirstName = user.FirstName,
                    LastName = user.LastName,
                };

                return Ok(userDto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while retrieving the user.");
                return StatusCode(500, "Internal server error");
            }
        }

        [HttpPost("create")]
        public async Task<ActionResult> CreateUser (CreateAppUserDto createUser)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            try
            {
                var appUser = new AppUser
                {
                    Id = Guid.NewGuid().ToString(),
                    Email = createUser.Email,
                    UserName = createUser.Email,
                    FirstName = createUser.FirstName,
                    LastName = createUser.LastName,
                };

                var result = await _userManager.CreateAsync(appUser, createUser.Password);

                if (!result.Succeeded)
                {
                    return BadRequest(result.Errors);
                }

                if (!await _roleManager.RoleExistsAsync("User"))
                {
                    await _roleManager.CreateAsync(new AppUserRoles { Name = "User" });
                }

                var roleResult = await _userManager.AddToRoleAsync(appUser, "User");

                await _context.SaveChangesAsync();

                return Ok("user created.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while creating an app user.");
                return StatusCode(500, "Internal server error");
            }
        }

        [HttpDelete("delete/{id}")]
        public async Task<ActionResult> DeleteUser ([FromRoute] Guid id)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            try
            {
                var user = await _userManager.FindByIdAsync(id.ToString());
                if (user == null)
                {
                    return NotFound("User not found.");
                }

                var result = await _userManager.DeleteAsync(user);
                if (!result.Succeeded)
                {
                    return BadRequest(result.Errors);
                }

                await _context.SaveChangesAsync();

                return Ok("User deleted successfully.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while deleting the user.");
                return StatusCode(500, "Internal server error");
            }
        }

        [HttpPost("create-admin")]
        public async Task<ActionResult> CreateAppUser (CreateAppUserDto createUser)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            try
            {
                var appUser = new AppUser
                {
                    Id = Guid.NewGuid().ToString(),
                    Email = createUser.Email,
                    UserName = createUser.Email,
                    FirstName = createUser.FirstName,
                    LastName = createUser.LastName,
                };

                var result = await _userManager.CreateAsync(appUser, createUser.Password);

                if (!result.Succeeded)
                {
                    return BadRequest(result.Errors);
                }

                // Assign the user to the Admin role
                if (!await _roleManager.RoleExistsAsync("Admin"))
                {
                    await _roleManager.CreateAsync(new AppUserRoles { Name = "Admin" });
                }

                var roleResult = await _userManager.AddToRoleAsync(appUser, "Admin");

                await _context.SaveChangesAsync();

                return Ok("admin created.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while creating an app user.");
                return StatusCode(500, "Internal server error");
            }
        }
    }
}