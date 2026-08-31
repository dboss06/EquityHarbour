using EquityHarbour.Data;
using EquityHarbour.DTOs;
using EquityHarbour.Models;
using Microsoft.EntityFrameworkCore;

namespace EquityHarbour.Services
{
    public class WalletService : IWalletService
    {
        private readonly ApplicationDbContext _context;
        public WalletService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<Wallet?> GetUserWalletAsync(string userId)
        {
            return await _context.Wallets.Include(w => w.Transactions).FirstOrDefaultAsync(w => w.UserId == userId);
        }
        public async Task<List<WalletTransactionDTO>> GetTransactionsAsync(string userId)
        {
            return await _context.WalletTransactions.Where(t => t.Wallet.UserId == userId).OrderByDescending(t => t.CreatedAt).Select(t => new WalletTransactionDTO
            {
                Id = t.Id,
                Amount = t.Amount,
                Type = t.Type.ToString(),
                Status = t.Status.ToString(),
                Reference = t.Reference,
                Description = t.Description,
                CreatedAt = t.CreatedAt,
                IsLocked = t.IsLocked,
                UnlockAt = t.UnlockAt
            }).ToListAsync();
        }

        public async Task<WalletTransaction> CreateTransactionAsync(int walletId, decimal amount, WalletTransactionType transactionType, WalletTransactionStatus transactionStatus, string? reference = null, string? description = null)
        {
            if(amount <= 0)
            {
                throw new ArgumentException("Transaction must be greater than 0");
            }
            var wallet = await _context.Wallets.FirstOrDefaultAsync(w => w.Id == walletId);
            if (wallet == null)
            {
                throw new InvalidOperationException("Wallet not found.");
            }
            var transaction = new WalletTransaction
            {
                WalletId = walletId,
                Amount = amount,
                Type = transactionType,
                Status = transactionStatus,
                Reference = reference,
                Description = description,
                CreatedAt = DateTime.UtcNow
            };
            _context.WalletTransactions.Add(transaction);
            await _context.SaveChangesAsync();
            return transaction;
        }

        public async Task CreditAsync(int walletId, decimal amount, WalletTransactionType transactionType, string description, string? reference = null)
        {
            if (amount <= 0)
            {
                throw new ArgumentException("Credit amount must be greater than zero.");
            }
            var wallet = await _context.Wallets.FirstOrDefaultAsync(w => w.Id == walletId);
            if (wallet == null)
            {
                throw new InvalidOperationException("Wallet not found.");
            }
            wallet.AvailableBalance += amount;
            if (transactionType == WalletTransactionType.Profit)
            {
                wallet.TotalProfit += amount;
            }
            var transaction = new WalletTransaction
            {
                WalletId = walletId,
                Amount = amount,
                Type = transactionType,
                Status = WalletTransactionStatus.Completed,
                Reference = reference,
                Description = description,
                CreatedAt = DateTime.UtcNow
            };
            _context.WalletTransactions.Add(transaction);
            await _context.SaveChangesAsync();
        }
        public async Task CreditLockedAsync(int walletId, decimal amount, WalletTransactionType transactionType, string description, string reference, DateTime unlockAt)
        {
            if (amount <= 0)
            {
                throw new ArgumentException("Credit amount must be greater than zero.");
            }
            var wallet = await _context.Wallets.FirstOrDefaultAsync(w => w.Id == walletId);
            if (wallet == null)
            {
                throw new InvalidOperationException("Wallet not found.");
            }
            wallet.LockedBalance += amount;
            if (transactionType == WalletTransactionType.Profit)
            {
                wallet.TotalProfit += amount;
            }
            var transaction = new WalletTransaction
            {
                WalletId = walletId,
                Amount = amount,
                Type = transactionType,
                Status = WalletTransactionStatus.Completed,
                Reference = reference,
                Description = description,
                CreatedAt = DateTime.UtcNow,
                IsLocked = true,
                UnlockAt = unlockAt
            };
            _context.WalletTransactions.Add(transaction);
            await _context.SaveChangesAsync();
        }

        public async Task UnlockFundsAsync(int walletId, decimal amount, string reference)
        {
            if (amount <= 0) return;

            var wallet = await _context.Wallets.FirstOrDefaultAsync(w => w.Id == walletId);
            if (wallet == null)
            {
                throw new InvalidOperationException("Wallet not found.");
            }
            wallet.LockedBalance -= amount;
            wallet.AvailableBalance += amount;

            var transaction = await _context.WalletTransactions
                .FirstOrDefaultAsync(t => t.WalletId == walletId && t.Reference == reference && t.IsLocked);
            if (transaction != null)
            {
                transaction.IsLocked = false;
            }

            await _context.SaveChangesAsync();
        }
    }
}
