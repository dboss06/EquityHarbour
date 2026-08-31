using EquityHarbour.Data;
using EquityHarbour.DTOs.Withdrawal;
using EquityHarbour.Models;
using EquityHarbour.Models.Enums;
using Microsoft.EntityFrameworkCore;

namespace EquityHarbour.Services
{
    public class WithdrawalService : IWithdrawalService
    {
        private readonly ApplicationDbContext _context; 
        private readonly IWithdrawalLimitService _limitService;

        public WithdrawalService(ApplicationDbContext context, IWithdrawalLimitService limitService)
        {
            _context = context;
            _limitService = limitService;
        }

        public async Task<WithdrawalDto> CreateAsync(string userId, CreateWithdrawalRequest request)
        {
            var totalInvested = await _context.Investments
                .Where(i => i.UserId == userId)
                .SumAsync(i => (decimal?)i.PrincipalAmount) ?? 0;

            var tier = await _limitService.GetApplicableTierAsync(totalInvested);
            if (tier == null)
            {
                throw new InvalidOperationException("You need to invest before you can request a withdrawal.");
            }
            if (request.Amount < tier.MinWithdrawalAmount || request.Amount > tier.MaxWithdrawalAmount)
            {
                throw new InvalidOperationException($"Based on your investment history, withdrawals must be between ₦{tier.MinWithdrawalAmount:N2} and ₦{tier.MaxWithdrawalAmount:N2}.");
            }
            if (request.Amount <= 0)
            {
                throw new ArgumentException("Withdrawal amount must be greater than zero.");
            }
            if (string.IsNullOrWhiteSpace(request.BankName))
            {
                throw new ArgumentException("Bank name is required.");
            }
            if (string.IsNullOrWhiteSpace(request.AccountName))
            {
                throw new ArgumentException("Account name is required.");
            }
            if (string.IsNullOrWhiteSpace(request.AccountNumber))
            {
                throw new ArgumentException("Account number is required.");
            }
            await using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                var wallet = await _context.Wallets.FirstOrDefaultAsync(w => w.UserId == userId);
                if (wallet == null)
                {
                    throw new InvalidOperationException("Wallet not found.");
                }
                if (wallet.AvailableBalance < request.Amount)
                {
                    throw new InvalidOperationException("Insufficient wallet balance.");
                }
                wallet.AvailableBalance -= request.Amount;
                //wallet.TotalWithdrawn += request.Amount;
                var withdrawal = new Withdrawal
                {
                    Amount = request.Amount,
                    Reference = GenerateReference(),
                    BankName = request.BankName.Trim(),
                    AccountName = request.AccountName.Trim(),
                    AccountNumber = request.AccountNumber.Trim(),
                    Description = request.Description,
                    Status = WithdrawalStatus.Pending,
                    CreatedAt = DateTime.UtcNow,
                    UserId = userId,
                    WalletId = wallet.Id
                };
                _context.Withdrawals.Add(withdrawal);
                var walletTransaction = new WalletTransaction
                {
                    WalletId = wallet.Id,
                    Amount = request.Amount,
                    Type = WalletTransactionType.Withdrawal,
                    Status = WalletTransactionStatus.Pending,
                    Reference = withdrawal.Reference,
                    Description = "Withdrawal request",
                    CreatedAt = DateTime.UtcNow
                };
                _context.WalletTransactions.Add(walletTransaction);
                await _context.SaveChangesAsync();
                await transaction.CommitAsync();
                return MapToDto(withdrawal);
            }
            catch (DbUpdateConcurrencyException)
            {
                await transaction.RollbackAsync();

                throw new InvalidOperationException(
                    "The wallet was updated by another transaction. Please try again.");
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }
        }
        public async Task<List<WithdrawalDto>> GetUserWithdrawalsAsync(string userId)
        {
            return await _context.Withdrawals.AsNoTracking().Where(w => w.UserId == userId).OrderByDescending(w => w.CreatedAt).Select(w => new WithdrawalDto{
                Id = w.Id,
                Amount = w.Amount,
                Reference = w.Reference,
                BankName = w.BankName,
                AccountName = w.AccountName,
                AccountNumber = w.AccountNumber,
                Status = w.Status,
                Description = w.Description,
                CreatedAt = w.CreatedAt,
                CompletedAt = w.CompletedAt
            }).ToListAsync();
        }
        public async Task<WithdrawalDto?> GetUserWithdrawalAsync(string userId, long id)
        {
            return await _context.Withdrawals.AsNoTracking().Where(w => w.Id == id && w.UserId == userId).Select(w => new WithdrawalDto{
                Id = w.Id,
                Amount = w.Amount,
                Reference = w.Reference,
                BankName = w.BankName,
                AccountName = w.AccountName,
                AccountNumber = w.AccountNumber,
                Status = w.Status,
                Description = w.Description,
                CreatedAt = w.CreatedAt,
                CompletedAt = w.CompletedAt
            }).FirstOrDefaultAsync();
        }
        public async Task<List<WithdrawalDto>> GetAllAsync()
        {
            return await _context.Withdrawals.AsNoTracking().Include(w => w.User).OrderByDescending(w => w.CreatedAt).Select(w => new WithdrawalDto
            {
                Id = w.Id,
                Amount = w.Amount,
                Reference = w.Reference,
                BankName = w.BankName,
                AccountName = w.AccountName,
                AccountNumber = w.AccountNumber,
                Status = w.Status,
                Description = w.Description,
                CreatedAt = w.CreatedAt,
                CompletedAt = w.CompletedAt,
                UserId = w.UserId,
                UserFullName = w.User.FullName
            }).ToListAsync();
        }
        public async Task<WithdrawalDto?> GetByIdAsync(long id)
        {
            return await _context.Withdrawals.AsNoTracking().Where(w => w.Id == id).Select(w => new WithdrawalDto{
                Id = w.Id,
                Amount = w.Amount,
                Reference = w.Reference,
                BankName = w.BankName,
                AccountName = w.AccountName,
                AccountNumber = w.AccountNumber,
                Status = w.Status,
                Description = w.Description,
                CreatedAt = w.CreatedAt,
                CompletedAt = w.CompletedAt
            }).FirstOrDefaultAsync();
        }
        public async Task<WithdrawalDto> ApproveAsync(long id)
        {
            await using var transaction = await _context.Database.BeginTransactionAsync();

            try
            {
                var withdrawal = await _context.Withdrawals.Include(w => w.User).FirstOrDefaultAsync(w => w.Id == id);
                if (withdrawal == null)
                {
                    throw new KeyNotFoundException("Withdrawal not found.");
                }
                if (withdrawal.Status != WithdrawalStatus.Pending)
                {
                    throw new InvalidOperationException("Only pending withdrawals can be approved.");
                }
                var wallet = await _context.Wallets.FirstOrDefaultAsync(w => w.Id == withdrawal.WalletId && w.UserId == withdrawal.UserId);
                var walletTransaction = await _context.WalletTransactions.FirstOrDefaultAsync(t => t.Reference == withdrawal.Reference && t.WalletId == withdrawal.WalletId && t.Type == WalletTransactionType.Withdrawal);
                if (walletTransaction == null)
                {
                    throw new InvalidOperationException("Withdrawal wallet transaction not found.");
                }
                withdrawal.Status = WithdrawalStatus.Completed;
                withdrawal.CompletedAt = DateTime.UtcNow;
                walletTransaction.Status = WalletTransactionStatus.Completed;
                wallet.TotalWithdrawn += withdrawal.Amount;
                await _context.SaveChangesAsync();
                await transaction.CommitAsync();
                return MapToDto(withdrawal);
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }
        }
        public async Task<WithdrawalDto> RejectAsync(long id)
        {
            await using var transaction = await _context.Database.BeginTransactionAsync();

            try
            {
                var withdrawal = await _context.Withdrawals.Include(w => w.User).FirstOrDefaultAsync(w => w.Id == id);
                if (withdrawal == null)
                {
                    throw new KeyNotFoundException("Withdrawal not found.");
                }
                if (withdrawal.Status != WithdrawalStatus.Pending)
                {
                    throw new InvalidOperationException("Only pending withdrawals can be rejected.");
                }
                var wallet = await _context.Wallets.FirstOrDefaultAsync(w => w.Id == withdrawal.WalletId && w.UserId == withdrawal.UserId);
                if (wallet == null)
                {
                    throw new InvalidOperationException("Wallet not found.");
                }
                var walletTransaction = await _context.WalletTransactions.FirstOrDefaultAsync(t => t.Reference == withdrawal.Reference && t.WalletId == withdrawal.WalletId && t.Type == WalletTransactionType.Withdrawal);
                if (walletTransaction == null)
                {
                    throw new InvalidOperationException("Withdrawal wallet transaction not found.");
                }
                wallet.AvailableBalance += withdrawal.Amount;
                //wallet.TotalWithdrawn -= withdrawal.Amount;
                withdrawal.Status = WithdrawalStatus.Rejected;
                walletTransaction.Status = WalletTransactionStatus.Reversed;
                await _context.SaveChangesAsync();
                await transaction.CommitAsync();
                return MapToDto(withdrawal);
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }
        }
        private static string GenerateReference()
        {
            return $"EH-WD-{DateTime.UtcNow:yyyyMMddHHmmss}-{Guid.NewGuid():N}".ToUpperInvariant();
        }

        private static WithdrawalDto MapToDto(Withdrawal withdrawal)
        {
            return new WithdrawalDto
            {
                Id = withdrawal.Id,
                Amount = withdrawal.Amount,
                Reference = withdrawal.Reference,
                BankName = withdrawal.BankName,
                AccountName = withdrawal.AccountName,
                AccountNumber = withdrawal.AccountNumber,
                Status = withdrawal.Status,
                Description = withdrawal.Description,
                CreatedAt = withdrawal.CreatedAt,
                CompletedAt = withdrawal.CompletedAt,
                UserId = withdrawal.UserId,
                UserFullName = withdrawal.User?.FullName ?? string.Empty
            };
        }
    }
}
