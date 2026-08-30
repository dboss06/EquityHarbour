using EquityHarbour.Data;
using EquityHarbour.DTOs.InvestmentPlans;
using EquityHarbour.Models;
using Microsoft.EntityFrameworkCore;
namespace EquityHarbour.Services
{
    public class InvestmentPlanService : IInvestmentPlanService
    {
        private readonly ApplicationDbContext _context;
        public InvestmentPlanService(ApplicationDbContext context)
        {
            _context = context;
        }
        public async Task<List<InvestmentPlanDto>> GetAllAsync() { 
            return await _context.InvestmentPlans.AsNoTracking().OrderByDescending(p => p.CreatedAt).Select(p => new InvestmentPlanDto{
                Id = p.Id,
                Name = p.Name,
                Description = p.Description,
                MinimumAmount = p.MinimumAmount,
                MaximumAmount = p.MaximumAmount,
                DurationDays = p.DurationDays,
                ReturnType = p.ReturnType,
                ReturnValue = p.ReturnValue,
                PayoutFrequency = p.PayoutFrequency,
                IsActive = p.IsActive,
                CreatedAt = p.CreatedAt,
                UpdatedAt = p.UpdatedAt
            }) .ToListAsync();
        }
        public async Task<List<InvestmentPlanDto>> GetActiveAsync()
        {
            return await _context.InvestmentPlans.AsNoTracking().Where(p => p.IsActive).OrderBy(p => p.MinimumAmount).Select(p => new InvestmentPlanDto{
                Id = p.Id,
                Name = p.Name,
                Description = p.Description,
                MinimumAmount = p.MinimumAmount,
                MaximumAmount = p.MaximumAmount,
                DurationDays = p.DurationDays,
                ReturnType = p.ReturnType,
                ReturnValue = p.ReturnValue,
                PayoutFrequency = p.PayoutFrequency,
                IsActive = p.IsActive,
                CreatedAt = p.CreatedAt,
                UpdatedAt = p.UpdatedAt
            }).ToListAsync();
        }
        public async Task<InvestmentPlanDto?> GetByIdAsync(int id)
        {
            return await _context.InvestmentPlans.AsNoTracking().Where(p => p.Id == id).Select(p => new InvestmentPlanDto{
                Id = p.Id,
                Name = p.Name,
                Description = p.Description,
                MinimumAmount = p.MinimumAmount,
                MaximumAmount = p.MaximumAmount,
                DurationDays = p.DurationDays,
                ReturnType = p.ReturnType,
                ReturnValue = p.ReturnValue,
                PayoutFrequency = p.PayoutFrequency,
                IsActive = p.IsActive,
                CreatedAt = p.CreatedAt,
                UpdatedAt = p.UpdatedAt
            }).FirstOrDefaultAsync();
        }
        public async Task<InvestmentPlanDto> CreateAsync(CreateInvestmentPlanRequest request)
        {
            ValidatePlan(request.Name, request.MinimumAmount, request.MaximumAmount, request.DurationDays, request.ReturnValue, request.ReturnType, request.PayoutFrequency);
            var nameExists = await _context.InvestmentPlans.AnyAsync(p => p.Name == request.Name);
            if (nameExists)
            {
                throw new InvalidOperationException("An investment plan with this name already exists.");
            }

            var plan = new InvestmentPlan
            {
                Name = request.Name.Trim(),
                Description = request.Description?.Trim(),
                MinimumAmount = request.MinimumAmount,
                MaximumAmount = request.MaximumAmount,
                DurationDays = request.DurationDays,
                ReturnType = request.ReturnType,
                ReturnValue = request.ReturnValue,
                PayoutFrequency = request.PayoutFrequency,
                IsActive = request.IsActive,
                CreatedAt = DateTime.UtcNow
            };
            _context.InvestmentPlans.Add(plan);
            await _context.SaveChangesAsync();
            return await GetByIdAsync(plan.Id) ?? throw new InvalidOperationException("Investment plan could not be retrieved after creation.");
        }
        public async Task<bool> UpdateAsync(int id, UpdateInvestmentPlanRequest request)
        {
            ValidatePlan(request.Name, request.MinimumAmount, request.MaximumAmount, request.DurationDays, request.ReturnValue, request.ReturnType, request.PayoutFrequency);
            var plan = await _context.InvestmentPlans.FirstOrDefaultAsync(p => p.Id == id);
            if (plan == null)
            {
                return false;
            }
            var hasInvestments = await _context.Investments.AnyAsync(i => i.InvestmentPlanId == id);
            if (hasInvestments)
            {
                throw new InvalidOperationException("This investment plan cannot be modified because it has already been used for an investment.");
            }
            var nameExists = await _context.InvestmentPlans.AnyAsync(p => p.Id != id && p.Name == request.Name);
            if (nameExists)
            {
                throw new InvalidOperationException("An investment plan with this name already exists.");
            }
            plan.Name = request.Name.Trim();
            plan.Description = request.Description?.Trim();
            plan.MinimumAmount = request.MinimumAmount;
            plan.MaximumAmount = request.MaximumAmount;
            plan.DurationDays = request.DurationDays;
            plan.ReturnType = request.ReturnType;
            plan.ReturnValue = request.ReturnValue;
            plan.PayoutFrequency = request.PayoutFrequency;
            plan.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();
            return true;
        }
        public async Task<bool> SetActiveAsync(int id, bool isActive)
        {
            var plan = await _context.InvestmentPlans.FirstOrDefaultAsync(p => p.Id == id);
            if (plan == null)
            {
                return false;
            }
            plan.IsActive = isActive;
            plan.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();
            return true;
        }
        private static void ValidatePlan(string name, decimal minimumAmount, decimal maximumAmount, int durationDays, decimal returnValue, InvestmentReturnType returnType, InvestmentPayoutFrequency payoutFrequency)
        {
            if (string.IsNullOrWhiteSpace(name))
            {
                throw new ArgumentException("Investment plan name is required.");
            }
            if (minimumAmount <= 0)
            {
                throw new ArgumentException("Minimum investment amount must be greater than zero.");
            }
            if (maximumAmount < minimumAmount)
            {
                throw new ArgumentException("Maximum investment amount cannot be less than the minimum amount.");
            }
            if (durationDays <= 0)
            {
                throw new ArgumentException("Duration must be greater than zero days.");
            }
            if (returnValue < 0)
            {
                throw new ArgumentException("Return value cannot be negative.");
            }
            if (returnType == InvestmentReturnType.Percentage && returnValue > 50)
            {
                throw new ArgumentException(
                    "Percentage return cannot exceed 50%.");
            }
            if (!Enum.IsDefined(payoutFrequency))
            {
                throw new ArgumentException("Invalid payout frequency.");
            }

            if (!Enum.IsDefined(returnType))
            {
                throw new ArgumentException("Invalid return type.");
            }
        }
        public async Task<bool> DeleteAsync(int id)
        {
            var plan = await _context.InvestmentPlans.FirstOrDefaultAsync(p => p.Id == id);
            if (plan == null)
            {
                return false;
            }
            var hasInvestments = await _context.Investments.AnyAsync(i => i.InvestmentPlanId == id);
            if (hasInvestments)
            {
                throw new InvalidOperationException("This investment plan cannot be deleted because it has already been used for an investment.");
            }
            _context.InvestmentPlans.Remove(plan);
            await _context.SaveChangesAsync();
            return true;
        }
    }
}
