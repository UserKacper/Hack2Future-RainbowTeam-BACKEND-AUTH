using FraudDetection.Contracts;
using FraudDetection.Database;
using FraudDetection.Database.Models;
using FraudDetection.DTOs;
using FraudDetection.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace backend.Controllers
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
        public async Task<IActionResult> Login([FromBody] LoginContract login)
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
        public async Task<IActionResult> VerifyEmail([FromQuery] string userId, [FromQuery] string token)
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
        public async Task<IActionResult> SendEmailConfirmation([FromQuery] string userId)
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



        [HttpPost("CreateUser")]
        public async Task<ActionResult> CreateUser(CreateAppUserDto createUser)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            try
            {
                var appUser = new AppUser
                {
                    Email = createUser.Email,
                    UserName = createUser.Email,
                    FirstName = createUser.FirstName,
                    LastName = createUser.LastName,
                    UniqueIdNumber = createUser.UniqueIdNumber
                };

                var result = await _userManager.CreateAsync(appUser, createUser.Password);

                if (!result.Succeeded)
                {
                    return BadRequest(result.Errors);
                }

                await _context.SaveChangesAsync();

                return Ok("user created.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while creating an app user.");
                return StatusCode(500, "Internal server error");
            }
        }

        [HttpPost("CreateAdmin")]
        public async Task<ActionResult> CreateAppUser(CreateAppUserDto createUser)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            try
            {
                var appUser = new AppUser
                {
                    Email = createUser.Email,
                    UserName = createUser.Email,
                    FirstName = createUser.FirstName,
                    LastName = createUser.LastName,
                    UniqueIdNumber = createUser.UniqueIdNumber
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