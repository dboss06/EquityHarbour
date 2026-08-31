using EquityHarbour.Data;
using EquityHarbour.Models;
using Microsoft.EntityFrameworkCore;

namespace EquityHarbour.Services
{
    public class InvestmentPayoutService : IInvestmentPayoutService
    {
        private readonly ApplicationDbContext _context;
        private readonly IWalletService _walletService;

        public InvestmentPayoutService(ApplicationDbContext context, IWalletService walletService)
        {
            _context = context;
            _walletService = walletService;
        }

        public async Task<int> ProcessPendingPayoutsAsync(long investmentId)
        {
            var investment = await _context.Investments.Include(i => i.Payouts).FirstOrDefaultAsync(i => i.Id == investmentId);
            if (investment == null)
            {
                throw new KeyNotFoundException("Investment not found.");
            }
            if (investment.Status != InvestmentStatus.Active)
            {
                throw new InvalidOperationException("Only active investments can receive payouts.");
            }
            if (investment.StartedAt == null || investment.MaturityDate == null)
            {
                throw new InvalidOperationException("Investment dates are not configured.");
            }
            if (investment.PayoutFrequency == InvestmentPayoutFrequency.AtMaturity)
            {
                return 0;
            }
            var now = DateTime.UtcNow;
            var periods = BuildPeriods(investment.StartedAt.Value, investment.MaturityDate.Value, investment.PayoutFrequency);
            var processed = 0;
            foreach (var period in periods)
            {
                if (period.End > now)
                    continue;
                var alreadyPaid = investment.Payouts.Any(p => p.PeriodStart == period.Start && p.PeriodEnd == period.End);
                if (alreadyPaid)
                    continue;
                var amount = CalculatePayoutAmount(investment, periods.Count, processed);
                if (amount <= 0)
                    continue;
                await using var transaction = await _context.Database.BeginTransactionAsync();
                try
                {
                    var freeWindowEnd = investment.StartedAt!.Value.AddDays(2);
                    DateTime? unlockAt = null;

                    if (period.End > freeWindowEnd)
                    {
                        var daysSinceFreeWindow = (period.End - freeWindowEnd).TotalDays;
                        var weeksElapsed = Math.Ceiling(daysSinceFreeWindow / 7.0);
                        unlockAt = freeWindowEnd.AddDays(weeksElapsed * 7);
                    }

                    var walletId = await GetWalletIdAsync(investment.UserId);
                    var reference = $"PAYOUT-{investment.Id}-{Guid.NewGuid():N}";

                    if (unlockAt == null)
                    {
                        await _walletService.CreditAsync(walletId, amount, WalletTransactionType.Profit,
                            $"Investment payout for investment #{investment.Id}", reference);
                    }
                    else
                    {
                        await _walletService.CreditLockedAsync(walletId, amount, WalletTransactionType.Profit,
                            $"Investment payout for investment #{investment.Id} (unlocks {unlockAt:MMM d, yyyy})", reference, unlockAt.Value);
                    }

                    var payout = new InvestmentPayout
                    {
                        InvestmentId = investment.Id,
                        Amount = amount,
                        Frequency = investment.PayoutFrequency,
                        PeriodStart = period.Start,
                        PeriodEnd = period.End,
                        PaidAt = now,
                        UnlockAt = unlockAt,
                        Unlocked = unlockAt == null,
                        Reference = reference
                    };
                    _context.InvestmentPayouts.Add(payout);
                    await _context.SaveChangesAsync();
                    await transaction.CommitAsync();
                    processed++;
                }
                catch
                {
                    await transaction.RollbackAsync();
                    throw;
                }
            }
            return processed;
        }

        public async Task<int> ProcessAllPendingPayoutsAsync()
        {
            var investments = await _context.Investments.Where(i => i.Status == InvestmentStatus.Active && i.PayoutFrequency != InvestmentPayoutFrequency.AtMaturity && i.StartedAt != null && i.MaturityDate != null).Select(i => i.Id).ToListAsync();
            var totalProcessed = 0;
            foreach (var investmentId in investments)
            {
                totalProcessed += await ProcessPendingPayoutsAsync(investmentId);
            }
            return totalProcessed;
        }
        public async Task<int> UnlockDuePayoutsAsync()
        {
            var now = DateTime.UtcNow;
            var duePayouts = await _context.InvestmentPayouts
                .Include(p => p.Investment)
                .Where(p => !p.Unlocked && p.UnlockAt != null && p.UnlockAt <= now)
                .ToListAsync();

            var unlockedCount = 0;
            foreach (var payout in duePayouts)
            {
                var walletId = await GetWalletIdAsync(payout.Investment.UserId);
                await _walletService.UnlockFundsAsync(walletId, payout.Amount, payout.Reference!);
                payout.Unlocked = true;
                unlockedCount++;
            }

            if (unlockedCount > 0)
            {
                await _context.SaveChangesAsync();
            }

            return unlockedCount;
        }
        private async Task<int> GetWalletIdAsync(string userId)
        {
            var wallet = await _context.Wallets.AsNoTracking().FirstOrDefaultAsync(w => w.UserId == userId);
            if (wallet == null)
            {
                throw new InvalidOperationException("Wallet not found.");
            }
            return wallet.Id;
        }
        private static List<PayoutPeriod> BuildPeriods(DateTime start, DateTime maturity, InvestmentPayoutFrequency frequency)
        {
            var periods = new List<PayoutPeriod>();

            var current = start;

            while (current < maturity)
            {
                var next = frequency switch
                {
                    InvestmentPayoutFrequency.Daily => current.AddDays(1),
                    InvestmentPayoutFrequency.Weekly => current.AddDays(7),
                    InvestmentPayoutFrequency.Monthly => current.AddMonths(1),

                    _ => maturity
                };

                if (next > maturity)
                    next = maturity;

                periods.Add(new PayoutPeriod(current, next));
                current = next;
            }

            return periods;
        }

        private static decimal CalculatePayoutAmount(Investment investment, int totalPeriods, int processedPeriods)
        {
            if (totalPeriods <= 0)
                return 0;

            var remainingProfit = investment.ExpectedReturn - investment.Payouts.Sum(p => p.Amount);
            if (remainingProfit <= 0)
                return 0;
            if (processedPeriods == totalPeriods - 1)
                return remainingProfit;
            var standardAmount =
                Math.Round(investment.ExpectedReturn / totalPeriods, 2, MidpointRounding.ToEven);

            return Math.Min(
                standardAmount,
                remainingProfit);
        }

        private sealed record PayoutPeriod(
            DateTime Start,
            DateTime End);
    }
}
