using EquityHarbour.Data;
using EquityHarbour.Models;
using Microsoft.EntityFrameworkCore;

namespace EquityHarbour.Services
{
    public class ReferralService : IReferralService
    {
        private readonly ApplicationDbContext _context;
        private readonly IWalletService _walletService;
        private static readonly decimal[] LevelPercentages = { 20m, 3m, 20m };

        public ReferralService(ApplicationDbContext context, IWalletService walletService)
        {
            _context = context;
            _walletService = walletService;
        }

        public async Task<string> GenerateUniqueReferralCodeAsync()
        {
            string code;
            do
            {
                code = Guid.NewGuid().ToString("N")[..8].ToUpperInvariant();
            } while (await _context.Users.AnyAsync(u => u.ReferralCode == code));
            return code;
        }

        public async Task<bool> LinkReferrerAsync(string newUserId, string? referralCode)
        {
            if (string.IsNullOrWhiteSpace(referralCode)) return false;

            var referrer = await _context.Users.FirstOrDefaultAsync(u => u.ReferralCode == referralCode);
            if (referrer == null || referrer.Id == newUserId) return false;

            var newUser = await _context.Users.FindAsync(newUserId);
            if (newUser == null) return false;

            newUser.ReferredByUserId = referrer.Id;
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task ProcessCommissionAsync(string sourceUserId, decimal amount, string sourceType)
        {
            var sourceUser = await _context.Users.FindAsync(sourceUserId);
            var currentReferrerId = sourceUser?.ReferredByUserId;

            for (int level = 0; level < LevelPercentages.Length && currentReferrerId != null; level++)
            {
                var referrer = await _context.Users.FindAsync(currentReferrerId);
                if (referrer == null) break;

                var commissionAmount = amount * LevelPercentages[level] / 100m;
                var wallet = await _context.Wallets.FirstOrDefaultAsync(w => w.UserId == referrer.Id);

                if (wallet != null && commissionAmount > 0)
                {
                    await _walletService.CreditAsync(
                        wallet.Id,
                        commissionAmount,
                        WalletTransactionType.ReferralCommission,
                        $"Level {level + 1} referral commission from {sourceType.ToLower()}",
                        $"REF-{sourceUserId}-{level + 1}-{Guid.NewGuid():N}");

                    _context.ReferralCommissions.Add(new ReferralCommission
                    {
                        ReferrerId = referrer.Id,
                        SourceUserId = sourceUserId,
                        Level = level + 1,
                        SourceAmount = amount,
                        CommissionAmount = commissionAmount,
                        SourceType = sourceType,
                        CreatedAt = DateTime.UtcNow
                    });
                }

                currentReferrerId = referrer.ReferredByUserId;
            }

            await _context.SaveChangesAsync();
        }

        public async Task<List<ApplicationUser>> GetDirectReferralsAsync(string userId)
        {
            return await _context.Users.Where(u => u.ReferredByUserId == userId).ToListAsync();
        }

        public async Task<List<ApplicationUser>> GetSecondLevelReferralsAsync(string userId)
        {
            var directIds = await _context.Users.Where(u => u.ReferredByUserId == userId).Select(u => u.Id).ToListAsync();
            if (!directIds.Any()) return new List<ApplicationUser>();
            return await _context.Users.Where(u => u.ReferredByUserId != null && directIds.Contains(u.ReferredByUserId)).ToListAsync();
        }

        public async Task<List<ApplicationUser>> GetThirdLevelReferralsAsync(string userId)
        {
            var secondIds = (await GetSecondLevelReferralsAsync(userId)).Select(u => u.Id).ToList();
            if (!secondIds.Any()) return new List<ApplicationUser>();
            return await _context.Users.Where(u => u.ReferredByUserId != null && secondIds.Contains(u.ReferredByUserId)).ToListAsync();
        }

        public async Task<int> GetDirectReferralsWithFirstDepositCountAsync(string userId)
        {
            var directIds = await _context.Users.Where(u => u.ReferredByUserId == userId).Select(u => u.Id).ToListAsync();
            if (!directIds.Any()) return 0;

            return await _context.Deposits
                .Where(d => directIds.Contains(d.UserId) && d.Status == DepositStatus.Completed)
                .Select(d => d.UserId)
                .Distinct()
                .CountAsync();
        }
        public async Task<int> GetQualifiedReferralCountAsync(string userId)
        {
            var directIds = await _context.Users.Where(u => u.ReferredByUserId == userId).Select(u => u.Id).ToListAsync();
            if (!directIds.Any()) return 0;

            var withDeposit = await _context.Deposits
                .Where(d => directIds.Contains(d.UserId) && d.Status == DepositStatus.Completed)
                .Select(d => d.UserId)
                .Distinct()
                .ToListAsync();

            var withInvestment = await _context.Investments
                .Where(i => directIds.Contains(i.UserId))
                .Select(i => i.UserId)
                .Distinct()
                .ToListAsync();

            return withDeposit.Intersect(withInvestment).Count();
        }
        public async Task<HashSet<string>> GetQualifiedUserIdsAsync(IEnumerable<string> userIds)
        {
            var ids = userIds.Distinct().ToList();
            if (!ids.Any()) return new HashSet<string>();

            var withDeposit = await _context.Deposits
                .Where(d => ids.Contains(d.UserId) && d.Status == DepositStatus.Completed)
                .Select(d => d.UserId)
                .Distinct()
                .ToListAsync();

            var withInvestment = await _context.Investments
                .Where(i => ids.Contains(i.UserId))
                .Select(i => i.UserId)
                .Distinct()
                .ToListAsync();

            return withDeposit.Intersect(withInvestment).ToHashSet();
        }
    }
}