using EquityHarbour.Data;
using EquityHarbour.DTOs.Tasks;
using EquityHarbour.Models;
using Microsoft.EntityFrameworkCore;

namespace EquityHarbour.Services
{
    public class TaskService : ITaskService
    {
        private readonly ApplicationDbContext _context;
        private readonly IReferralService _referralService;
        private readonly IWalletService _walletService;

        // Fixed milestone list — matches the original mockup's ten reward tiers.
        private static readonly (int Target, decimal Reward)[] Milestones =
        {
            (5, 1000m), (10, 2000m), (20, 3000m), (30, 4000m), (50, 5000m),
            (60, 6000m), (70, 7000m), (80, 8000m), (90, 9000m), (100, 10000m)
        };

        public TaskService(ApplicationDbContext context, IReferralService referralService, IWalletService walletService)
        {
            _context = context;
            _referralService = referralService;
            _walletService = walletService;
        }

        public async Task<List<TaskMilestoneDto>> GetMilestonesAsync(string userId)
        {
            var currentCount = await _referralService.GetDirectReferralsWithFirstDepositCountAsync(userId);
            var claimedTargets = await _context.TaskClaims
                .Where(c => c.UserId == userId)
                .Select(c => c.MilestoneTarget)
                .ToListAsync();

            return Milestones.Select(m => new TaskMilestoneDto
            {
                Target = m.Target,
                RewardAmount = m.Reward,
                CurrentCount = currentCount,
                IsClaimed = claimedTargets.Contains(m.Target)
            }).ToList();
        }

        public async Task<decimal> ClaimAsync(string userId, int target)
        {
            var milestone = Milestones.FirstOrDefault(m => m.Target == target);
            if (milestone == default)
            {
                throw new ArgumentException("Invalid milestone.");
            }

            var alreadyClaimed = await _context.TaskClaims.AnyAsync(c => c.UserId == userId && c.MilestoneTarget == target);
            if (alreadyClaimed)
            {
                throw new InvalidOperationException("This reward has already been claimed.");
            }

            var currentCount = await _referralService.GetDirectReferralsWithFirstDepositCountAsync(userId);
            if (currentCount < target)
            {
                throw new InvalidOperationException("You haven't reached this milestone yet.");
            }

            var wallet = await _context.Wallets.FirstOrDefaultAsync(w => w.UserId == userId);
            if (wallet == null)
            {
                throw new InvalidOperationException("Wallet not found.");
            }

            _context.TaskClaims.Add(new TaskClaim
            {
                UserId = userId,
                MilestoneTarget = target,
                RewardAmount = milestone.Reward,
                ClaimedAt = DateTime.UtcNow
            });

            await _walletService.CreditAsync(
                wallet.Id,
                milestone.Reward,
                WalletTransactionType.TaskReward,
                $"Task reward: {target} referrals milestone",
                $"TASK-{userId}-{target}");

            await _context.SaveChangesAsync();

            return milestone.Reward;
        }
    }
}