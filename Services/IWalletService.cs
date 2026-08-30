using EquityHarbour.DTOs;
using EquityHarbour.Models;

namespace EquityHarbour.Services
{
    public interface IWalletService
    {
        Task<Wallet?> GetUserWalletAsync(string UserId);
        Task<List<WalletTransactionDTO>> GetTransactionsAsync(string userId);
        Task<WalletTransaction> CreateTransactionAsync(
                int wallwtId,
                decimal amount,
                WalletTransactionType transactionType,
                WalletTransactionStatus transactionStatus,
                string? reference = null,
                string? description = null
            );
        Task CreditAsync(int walletId, decimal amount, WalletTransactionType transactionType, string description, string? reference = null);
    }
}
