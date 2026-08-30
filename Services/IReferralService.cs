using EquityHarbour.Models;

namespace EquityHarbour.Services
{
    public interface IReferralService
    {
        Task<string> GenerateUniqueReferralCodeAsync();
        Task<bool> LinkReferrerAsync(string newUserId, string? referralCode);
        Task ProcessCommissionAsync(string sourceUserId, decimal amount, string sourceType);
        Task<List<ApplicationUser>> GetDirectReferralsAsync(string userId);
        Task<List<ApplicationUser>> GetSecondLevelReferralsAsync(string userId);
        Task<List<ApplicationUser>> GetThirdLevelReferralsAsync(string userId);
        Task<int> GetDirectReferralsWithFirstDepositCountAsync(string userId);
    }
}