using FraudDetection.Database.Models;

namespace FraudDetection.Services
{
    public interface ITokenService
    {
        Task<string> CreateJWT (AppUser appUser);
    }
}