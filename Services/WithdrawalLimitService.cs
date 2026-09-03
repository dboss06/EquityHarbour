using EquityHarbour.Data;
using EquityHarbour.DTOs.WithdrawalLimits;
using EquityHarbour.Models;
using Microsoft.EntityFrameworkCore;

namespace EquityHarbour.Services
{
    public class WithdrawalLimitService : IWithdrawalLimitService
    {
        private readonly ApplicationDbContext _context;

        public WithdrawalLimitService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<List<WithdrawalLimitTierDto>> GetAllAsync()
        {
            return await _context.WithdrawalLimitTiers
                .AsNoTracking()
                .OrderBy(t => t.MinInvestedAmount)
                .Select(t => MapToDto(t))
                .ToListAsync();
        }

        public async Task<WithdrawalLimitTierDto> CreateAsync(CreateWithdrawalLimitTierRequest request)
        {
            if (request.MinWithdrawalAmount <= 0)
                throw new ArgumentException("Minimum withdrawal amount must be greater than zero.");
            if (request.MaxWithdrawalAmount < request.MinWithdrawalAmount)
                throw new ArgumentException("Maximum withdrawal amount cannot be less than the minimum.");
            if (request.MaxInvestedAmount.HasValue && request.MaxInvestedAmount < request.MinInvestedAmount)
                throw new ArgumentException("Maximum invested amount cannot be less than the minimum.");

            var tier = new WithdrawalLimitTier
            {
                MinInvestedAmount = request.MinInvestedAmount,
                MaxInvestedAmount = request.MaxInvestedAmount,
                MinWithdrawalAmount = request.MinWithdrawalAmount,
                MaxWithdrawalAmount = request.MaxWithdrawalAmount,
                MinReferralCount = request.MinReferralCount,
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            };

            _context.WithdrawalLimitTiers.Add(tier);
            await _context.SaveChangesAsync();
            return MapToDto(tier);
        }

        public async Task<bool> SetActiveAsync(int id, bool isActive)
        {
            var tier = await _context.WithdrawalLimitTiers.FirstOrDefaultAsync(t => t.Id == id);
            if (tier == null) return false;
            tier.IsActive = isActive;
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var tier = await _context.WithdrawalLimitTiers.FirstOrDefaultAsync(t => t.Id == id);
            if (tier == null) return false;
            _context.WithdrawalLimitTiers.Remove(tier);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<WithdrawalLimitTierDto?> GetApplicableTierAsync(decimal totalInvested)
        {
            var tier = await _context.WithdrawalLimitTiers
                .AsNoTracking()
                .Where(t => t.IsActive
                    && totalInvested >= t.MinInvestedAmount
                    && (t.MaxInvestedAmount == null || totalInvested <= t.MaxInvestedAmount))
                .OrderByDescending(t => t.MinInvestedAmount)
                .FirstOrDefaultAsync();

            return tier == null ? null : MapToDto(tier);
        }

        private static WithdrawalLimitTierDto MapToDto(WithdrawalLimitTier t) => new()
        {
            Id = t.Id,
            MinInvestedAmount = t.MinInvestedAmount,
            MaxInvestedAmount = t.MaxInvestedAmount,
            MinWithdrawalAmount = t.MinWithdrawalAmount,
            MaxWithdrawalAmount = t.MaxWithdrawalAmount,
            IsActive = t.IsActive,
            MinReferralCount = t.MinReferralCount,
        };
    }
}