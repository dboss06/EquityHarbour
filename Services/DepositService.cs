using EquityHarbour.Data;
using EquityHarbour.DTOs.Deposits;
using EquityHarbour.Models;
using Microsoft.EntityFrameworkCore;

namespace EquityHarbour.Services
{
    public class DepositService : IDepositService
    {
        private readonly ApplicationDbContext _context;
        private readonly IDepositAccountService _accountService;
        private readonly IReferralService _referralService;
        private readonly ILogger<DepositService> _logger;

        public DepositService(ApplicationDbContext context, IDepositAccountService accountService, IReferralService referralService, ILogger<DepositService> logger)
        {
            _context = context;
            _accountService = accountService;
            _referralService = referralService;
            _logger = logger;
        }

        public async Task<DepositDTO> CreateAsync(string userId, CreateDepositRequest request)
        {
            if (request.Amount <= 0)
            {
                throw new ArgumentException("Deposit amount must be greater than 0");
            }
            var wallet = await _context.Wallets.FirstOrDefaultAsync(w => w.UserId == userId);
            if (wallet == null)
            {
                throw new InvalidOperationException("Wallet Not Found");
            }

            string? accountBankName = null;
            string? accountNumber = null;
            string? accountName = null;

            if (request.DepositAccountId.HasValue)
            {
                var accounts = await _accountService.GetAllAsync();
                var account = accounts.FirstOrDefault(a => a.Id == request.DepositAccountId.Value);
                if (account != null)
                {
                    accountBankName = account.BankName;
                    accountNumber = account.AccountNumber;
                    accountName = account.AccountName;
                }
            }

            var reference = GenerateReference();
            var deposit = new Deposit
            {
                Amount = request.Amount,
                Reference = reference,
                Description = request.Description,
                Status = DepositStatus.Pending,
                CreatedAt = DateTime.UtcNow,
                UserId = userId,
                WalletId = wallet.Id,
                AccountBankName = accountBankName,
                AccountNumber = accountNumber,
                AccountName = accountName
            };
            _context.Deposits.Add(deposit);
            await _context.SaveChangesAsync();
            return MapToDto(deposit);
        }
        public async Task<List<DepositDTO>> GetUserDepositsAsync(string userId)
        {
            return await _context.Deposits.AsNoTracking().Where(d => d.UserId == userId).OrderByDescending(d => d.CreatedAt).Select(d => new DepositDTO{
                    Id = d.Id,
                    Amount = d.Amount,
                    Reference = d.Reference,
                    Description = d.Description,
                    Status = d.Status,
                    CreatedAt = d.CreatedAt,
                    CompletedAt = d.CompletedAt
                }).ToListAsync();
        }
        public async Task<DepositDTO?> GetUserDepositAsync(string userId, long id)
        {
            return await _context.Deposits.AsNoTracking().Where(d => d.Id == id && d.UserId == userId).Select(d => new DepositDTO{
                Id = d.Id,
                Amount = d.Amount,
                Reference = d.Reference,
                Description = d.Description,
                Status = d.Status,
                CreatedAt = d.CreatedAt,
                CompletedAt = d.CompletedAt
            }).FirstOrDefaultAsync();
        }
        public async Task<List<DepositDTO>> GetAllAsync()
        {
            return await _context.Deposits.AsNoTracking().Include(d => d.User).OrderByDescending(d => d.CreatedAt).Select(d => new DepositDTO
            {
                Id = d.Id,
                Amount = d.Amount,
                Reference = d.Reference,
                Description = d.Description,
                Status = d.Status,
                CreatedAt = d.CreatedAt,
                CompletedAt = d.CompletedAt,
                AccountBankName = d.AccountBankName,
                AccountNumber = d.AccountNumber,
                AccountName = d.AccountName,
                UserId = d.UserId,
                UserFullName = d.User.FullName
            }).ToListAsync();
        }
        public async Task<DepositDTO?> GetByIdAsync(long id)
        {
            return await _context.Deposits.AsNoTracking().Where(d => d.Id == id).Select(d => new DepositDTO{
                    Id = d.Id,
                    Amount = d.Amount,
                    Reference = d.Reference,
                    Description = d.Description,
                    Status = d.Status,
                    CreatedAt = d.CreatedAt,
                    CompletedAt = d.CompletedAt
                }).FirstOrDefaultAsync();
        }
        public async Task<DepositDTO> ApproveAsync(long id)
        {
            await using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                var deposit = await _context.Deposits.Include(d => d.User).FirstOrDefaultAsync(d => d.Id == id);
                if (deposit == null)
                {
                    throw new KeyNotFoundException("Deposit not found.");
                }
                if (deposit.Status != DepositStatus.Pending)
                {
                    throw new InvalidOperationException("Only pending deposits can be approved.");
                }
                var wallet = await _context.Wallets.FirstOrDefaultAsync(w => w.Id == deposit.WalletId && w.UserId == deposit.UserId);
                if (wallet == null)
                {
                    throw new InvalidOperationException(
                        "Wallet not found.");
                }
                wallet.AvailableBalance += deposit.Amount;
                wallet.TotalDeposited += deposit.Amount;
                deposit.Status = DepositStatus.Completed;
                deposit.CompletedAt = DateTime.UtcNow;
                var walletTransaction = new WalletTransaction
                {
                    WalletId = wallet.Id,
                    Amount = deposit.Amount,
                    Type = WalletTransactionType.Deposit,
                    Status = WalletTransactionStatus.Completed,
                    Reference = deposit.Reference,
                    Description = "Bank transfer deposit",
                    CreatedAt = DateTime.UtcNow
                };
                _context.WalletTransactions.Add(walletTransaction);
                await _context.SaveChangesAsync();
                await transaction.CommitAsync();
                try
                {
                    await _referralService.ProcessCommissionAsync(deposit.UserId, deposit.Amount, "Deposit");
                }
                catch (Exception ex)
                {
                    _logger?.LogError(ex, "Referral commission processing failed for deposit {DepositId}", deposit.Id);
                }
                return MapToDto(deposit);
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }
        }
        public async Task<DepositDTO> RejectAsync(long id)
        {
            var deposit = await _context.Deposits.Include(d => d.User).FirstOrDefaultAsync(d => d.Id == id);
            if (deposit == null)
            {
                throw new KeyNotFoundException("Deposit not found.");
            }
            if (deposit.Status != DepositStatus.Pending)
            {
                throw new InvalidOperationException("Only pending deposits can be rejected.");
            }
            deposit.Status = DepositStatus.Rejected;
            await _context.SaveChangesAsync();
            return MapToDto(deposit);
        }
        private static string GenerateReference()
        {
            return $"EH-{DateTime.UtcNow:yyyyMMddHHmmss}-{Guid.NewGuid():N}".ToUpperInvariant();
        }
        private static DepositDTO MapToDto(Deposit deposit)
        {
            return new DepositDTO
            {
                Id = deposit.Id,
                Amount = deposit.Amount,
                Reference = deposit.Reference,
                Description = deposit.Description,
                Status = deposit.Status,
                CreatedAt = deposit.CreatedAt,
                CompletedAt = deposit.CompletedAt,
                AccountBankName = deposit.AccountBankName,
                AccountNumber = deposit.AccountNumber,
                AccountName = deposit.AccountName,
                UserId = deposit.UserId,
                UserFullName = deposit.User?.FullName ?? string.Empty
            };
        }
    }
}
