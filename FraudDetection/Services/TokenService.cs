using FraudDetection.Database.Models;
using Microsoft.AspNetCore.Identity;
using System.Security.Claims;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using System.IdentityModel.Tokens.Jwt;
namespace FraudDetection.Services
{
    public class TokenService : ITokenService
    {
        private readonly ILogger<TokenService> _logger;
        private readonly IConfiguration _configuration;
        private readonly UserManager<AppUser> _userManager;
        public TokenService (ILogger<TokenService> logger, UserManager<AppUser> userManager, IConfiguration configuration)
        {
            _logger = logger;
            _userManager = userManager;
            _configuration = configuration;
        }

        public async Task<string> CreateJWT (AppUser appUser)
        {
            try
            {
                // Define token expiration time
                var expirationTime = DateTime.UtcNow.AddHours(1);

                // Create token handler
                var tokenHandler = new JwtSecurityTokenHandler();

                // Define token descriptor
                var tokenDescriptor = new SecurityTokenDescriptor
                {
                    Subject = new ClaimsIdentity(new[]
                    {
                        new Claim(ClaimTypes.Email, appUser.Email),
                        new Claim(ClaimTypes.NameIdentifier, appUser.Id.ToString()),
                        new Claim(ClaimTypes.Role, (await _userManager.GetRolesAsync(appUser)).FirstOrDefault() ?? string.Empty),
                    }),

                    Expires = expirationTime,
                    SigningCredentials = new SigningCredentials(
                        new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["secret:jwt"])),
                        SecurityAlgorithms.HmacSha256Signature)
                };

                // Create token
                var token = tokenHandler.CreateToken(tokenDescriptor);

                // Log token creation
                _logger.LogInformation("JWT created successfully.");

                // Return token
                return tokenHandler.WriteToken(token);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating JWT.");
                throw;
            }
        }
    }
}
