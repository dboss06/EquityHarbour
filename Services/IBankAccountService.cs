using EquityHarbour.DTOs.BankAccounts;

namespace EquityHarbour.Services
{
    public interface IBankAccountService
    {
        Task<BankAccountDto?> GetByUserIdAsync(string userId);
        Task<BankAccountDto> UpsertAsync(string userId, SaveBankAccountRequest request);
    }
}