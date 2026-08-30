    using EquityHarbour.Data;
    using EquityHarbour.DTOs.Investments;
    using EquityHarbour.Models;
    using Microsoft.EntityFrameworkCore;

    namespace EquityHarbour.Services
    {

        public class InvestmentService : IInvestmentService
        {
            private readonly ApplicationDbContext _context;
            private readonly IReferralService _referralService;
            private readonly ILogger<InvestmentService> _logger;

            public InvestmentService(ApplicationDbContext context, IReferralService referralService, ILogger<InvestmentService> logger)
            {
                _context = context;
                _referralService = referralService;
                _logger = logger;
            }
            public async Task<InvestmentDto> CreateAsync(string userId, CreateInvestmentRequest request)
            {
                if (request.Amount <= 0)
                {
                    throw new ArgumentException("Investment amount must be greater than zero.");
                }
                var plan = await _context.InvestmentPlans.FirstOrDefaultAsync(p => p.Id == request.InvestmentPlanId && p.IsActive);
                if (plan == null)
                {
                    throw new InvalidOperationException("Investment plan not found or is inactive.");
                }
                if (request.Amount < plan.MinimumAmount)
                {
                    throw new ArgumentException($"Minimum investment amount is {plan.MinimumAmount:N2}.");
                }
                if (request.Amount > plan.MaximumAmount)
                {
                    throw new ArgumentException($"Maximum investment amount is {plan.MaximumAmount:N2}.");
                }
                var wallet = await _context.Wallets.FirstOrDefaultAsync(w => w.UserId == userId);
                if (wallet == null)
                {
                    throw new InvalidOperationException("Wallet not found.");
                }
                if (wallet.AvailableBalance < request.Amount)
                {
                    throw new InvalidOperationException("Insufficient wallet balance.");
                }
                decimal expectedReturn;
                if (plan.ReturnType == InvestmentReturnType.Percentage)
                {
                    expectedReturn = request.Amount * plan.ReturnValue / 100m;
                }
                else
                {
                    expectedReturn = plan.ReturnValue;
                }

                var startedAt = DateTime.UtcNow;

                var maturityDate = startedAt.AddDays(
                    plan.DurationDays);

                await using var transaction =
                    await _context.Database.BeginTransactionAsync();

                try
                {
                    wallet.AvailableBalance -= request.Amount;
                    wallet.InvestedBalance += request.Amount;

                    var investment = new Investment
                    {
                        UserId = userId,

                        InvestmentPlanId = plan.Id,

                        PrincipalAmount = request.Amount,

                        ReturnType = plan.ReturnType,

                        ReturnValue = plan.ReturnValue,

                        ExpectedReturn = expectedReturn,

                        DurationDays = plan.DurationDays,

                        PayoutFrequency = plan.PayoutFrequency,

                        Status = InvestmentStatus.Active,

                        CreatedAt = startedAt,

                        StartedAt = startedAt,

                        MaturityDate = maturityDate
                    };

                    _context.Investments.Add(investment);

                    var walletTransaction = new WalletTransaction
                    {
                        WalletId = wallet.Id,

                        Amount = request.Amount,

                        Type = WalletTransactionType.Investment,

                        Status = WalletTransactionStatus.Completed,

                        Reference = Guid.NewGuid().ToString("N"),

                        Description =
                            $"Investment in {plan.Name}",

                        CreatedAt = startedAt
                    };

                    _context.WalletTransactions.Add(
                        walletTransaction);

                    await _context.SaveChangesAsync();

                    await transaction.CommitAsync();
                    try
                    {
                        await _referralService.ProcessCommissionAsync(userId, request.Amount, "Investment");
                    }
                    catch(Exception ex)
                    {
                        _logger?.LogError(ex, "Referral commission processing failed for deposit {DepositId}", investment.Id);
                    }
                    return new InvestmentDto
                    {
                        Id = investment.Id,

                        InvestmentPlanId = plan.Id,

                        InvestmentPlanName = plan.Name,

                        PrincipalAmount =
                            investment.PrincipalAmount,

                        ReturnType =
                            investment.ReturnType,

                        ReturnValue =
                            investment.ReturnValue,

                        ExpectedReturn =
                            investment.ExpectedReturn,

                        DurationDays =
                            investment.DurationDays,

                        PayoutFrequency =
                            investment.PayoutFrequency,

                        Status =
                            investment.Status,

                        CreatedAt =
                            investment.CreatedAt,

                        StartedAt =
                            investment.StartedAt,

                        MaturityDate =
                            investment.MaturityDate,

                        CompletedAt =
                            investment.CompletedAt
                    };
                }
                catch
                {
                    await transaction.RollbackAsync();
                    throw;
                }
            }

            public async Task<List<InvestmentDto>>
                GetUserInvestmentsAsync(string userId)
            {
                return await _context.Investments
                    .AsNoTracking()
                    .Where(i => i.UserId == userId)
                    .OrderByDescending(i => i.CreatedAt)
                    .Select(i => new InvestmentDto
                    {
                        Id = i.Id,

                        InvestmentPlanId =
                            i.InvestmentPlanId,

                        InvestmentPlanName =
                            i.InvestmentPlan.Name,

                        PrincipalAmount =
                            i.PrincipalAmount,

                        ReturnType =
                            i.ReturnType,

                        ReturnValue =
                            i.ReturnValue,

                        ExpectedReturn =
                            i.ExpectedReturn,

                        DurationDays =
                            i.DurationDays,

                        PayoutFrequency =
                            i.PayoutFrequency,

                        Status =
                            i.Status,

                        CreatedAt =
                            i.CreatedAt,

                        StartedAt =
                            i.StartedAt,

                        MaturityDate =
                            i.MaturityDate,

                        CompletedAt =
                            i.CompletedAt
                    })
                    .ToListAsync();
            }
        public async Task<List<InvestmentDto>> GetAllAsync()
        {
            return await _context.Investments.AsNoTracking().Include(i => i.User).Include(i => i.InvestmentPlan).OrderByDescending(i => i.CreatedAt).Select(i => new InvestmentDto
                {
                    Id = i.Id,
                    UserId = i.UserId,
                    InvestmentPlanId = i.InvestmentPlanId,
                    InvestmentPlanName = i.InvestmentPlan.Name,
                    UserFullName = i.User.FullName,
                    PrincipalAmount = i.PrincipalAmount,
                    ReturnType = i.ReturnType,
                    ReturnValue = i.ReturnValue,
                    ExpectedReturn = i.ExpectedReturn,
                    DurationDays = i.DurationDays,
                    PayoutFrequency = i.PayoutFrequency,
                    Status = i.Status,
                    CreatedAt = i.CreatedAt,
                    StartedAt = i.StartedAt,
                    MaturityDate = i.MaturityDate,
                    CompletedAt = i.CompletedAt
                })
                .ToListAsync();
        }
        public async Task<InvestmentDto?> GetUserInvestmentAsync(string userId, long id)
            {
                return await _context.Investments
                    .AsNoTracking()
                    .Where(i =>
                        i.Id == id &&
                        i.UserId == userId)
                    .Select(i => new InvestmentDto
                    {
                        Id = i.Id,

                        InvestmentPlanId =
                            i.InvestmentPlanId,

                        InvestmentPlanName =
                            i.InvestmentPlan.Name,

                        PrincipalAmount =
                            i.PrincipalAmount,

                        ReturnType =
                            i.ReturnType,

                        ReturnValue =
                            i.ReturnValue,

                        ExpectedReturn =
                            i.ExpectedReturn,

                        DurationDays =
                            i.DurationDays,

                        PayoutFrequency =
                            i.PayoutFrequency,

                        Status =
                            i.Status,

                        CreatedAt =
                            i.CreatedAt,

                        StartedAt =
                            i.StartedAt,

                        MaturityDate =
                            i.MaturityDate,

                        CompletedAt =
                            i.CompletedAt
                    })
                    .FirstOrDefaultAsync();
            }
            public async Task<InvestmentDto> ProcessMaturityAsync(long investmentId)
            {
                await using var transaction = await _context.Database.BeginTransactionAsync();

                try
                {
                    var investment = await _context.Investments.Include(i => i.Payouts).Include(i => i.InvestmentPlan).FirstOrDefaultAsync(i => i.Id == investmentId);
                    if (investment == null)
                    {
                        throw new KeyNotFoundException("Investment not found.");
                    }
                    if (investment.Status != InvestmentStatus.Active)
                    {
                        throw new InvalidOperationException("Only active investments can be processed.");
                    }
                    if (investment.MaturityDate == null)
                    {
                        throw new InvalidOperationException("Investment does not have a maturity date.");
                    }
                    if (investment.MaturityDate > DateTime.UtcNow)
                    {
                        throw new InvalidOperationException("Investment has not reached maturity.");
                    }
                    var wallet = await _context.Wallets.FirstOrDefaultAsync(w => w.UserId == investment.UserId);
                    if (wallet == null)
                    {
                        throw new InvalidOperationException("Wallet not found.");
                    }
                    wallet.InvestedBalance -= investment.PrincipalAmount;
                    wallet.AvailableBalance += investment.PrincipalAmount;
                    var alreadyPaid = investment.Payouts.Sum(p => p.Amount);
                    var remainingProfit = investment.ExpectedReturn - alreadyPaid;
                    if (remainingProfit < 0)
                    {
                        remainingProfit = 0;
                    }
                    if (remainingProfit > 0)
                    {
                        wallet.AvailableBalance += remainingProfit;
                        wallet.TotalProfit += remainingProfit;
                        var profitTransaction = new WalletTransaction
                        {
                            WalletId = wallet.Id,
                            Amount = remainingProfit,
                            Type = WalletTransactionType.Profit,
                            Status = WalletTransactionStatus.Completed,
                            Reference = $"PROFIT-{investment.Id}-{Guid.NewGuid():N}",
                            Description = $"Investment profit for investment #{investment.Id}",
                            CreatedAt = DateTime.UtcNow
                        };
                        _context.WalletTransactions.Add(profitTransaction);
                    }
                    var principalTransaction = new WalletTransaction
                    {
                        WalletId = wallet.Id,
                        Amount = investment.PrincipalAmount,
                        Type = WalletTransactionType.InvestmentReturn,
                        Status = WalletTransactionStatus.Completed,
                        Reference = $"RETURN-{investment.Id}-{Guid.NewGuid():N}",
                        Description = $"Principal returned for investment #{investment.Id}",
                        CreatedAt = DateTime.UtcNow
                    };
                    _context.WalletTransactions.Add(principalTransaction);
                    investment.Status = InvestmentStatus.Completed;
                    investment.CompletedAt = DateTime.UtcNow;
                    await _context.SaveChangesAsync();
                    await transaction.CommitAsync();
                    return MapToDto(investment);
                }
                catch
                {
                    await transaction.RollbackAsync();
                    throw;
                }
            }
            public async Task<List<InvestmentDto>> ProcessMaturedInvestmentsAsync()
            {
                var maturedInvestments = await _context.Investments.Where(i => i.Status == InvestmentStatus.Active && i.MaturityDate != null && i.MaturityDate <= DateTime.UtcNow).Select(i => i.Id).ToListAsync(); 
                var processed = new List<InvestmentDto>();
                foreach (var investmentId in maturedInvestments)
                {
                    try
                    {
                        var investment = await ProcessMaturityAsync(investmentId);
                        processed.Add(investment);
                    }
                    catch (InvalidOperationException)
                    {
                    
                    }
                }

                return processed;
            }
            private static InvestmentDto MapToDto(Investment investment)
            {
                return new InvestmentDto
                {
                    Id = investment.Id,
                    InvestmentPlanId = investment.InvestmentPlanId,
                    InvestmentPlanName = investment.InvestmentPlan?.Name ?? string.Empty,
                    PrincipalAmount = investment.PrincipalAmount,
                    ReturnType = investment.ReturnType,
                    ReturnValue = investment.ReturnValue,
                    ExpectedReturn = investment.ExpectedReturn,
                    DurationDays = investment.DurationDays,
                    PayoutFrequency = investment.PayoutFrequency,
                    Status = investment.Status,
                    CreatedAt = investment.CreatedAt,
                    StartedAt = investment.StartedAt,
                    MaturityDate = investment.MaturityDate,
                    CompletedAt = investment.CompletedAt
                };
            }
            public async Task<InvestmentDto> PrepareTestPayoutPeriodAsync(
        long investmentId)
            {
                var investment = await _context.Investments
                    .Include(i => i.InvestmentPlan)
                    .FirstOrDefaultAsync(i => i.Id == investmentId);

                if (investment == null)
                {
                    throw new KeyNotFoundException(
                        "Investment not found.");
                }

                if (investment.Status != InvestmentStatus.Active)
                {
                    throw new InvalidOperationException(
                        "Only active investments can be tested.");
                }

                if (investment.PayoutFrequency ==
                    InvestmentPayoutFrequency.AtMaturity)
                {
                    throw new InvalidOperationException(
                        "This investment does not have periodic payouts.");
                }

                var now = DateTime.UtcNow;

                investment.StartedAt =
                    now.AddDays(-2);

                investment.MaturityDate =
                    now.AddDays(investment.DurationDays - 2);

                await _context.SaveChangesAsync();

                return MapToDto(investment);
            }
        }
    }