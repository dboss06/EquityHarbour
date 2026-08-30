using EquityHarbour.DTOs.DepositAccounts;

namespace EquityHarbour.Services
{
    public interface IDepositAccountService
    {
        Task<List<DepositAccountDto>> GetAllAsync();
        Task<List<DepositAccountDto>> GetActiveAsync();
        Task<DepositAccountDto> CreateAsync(CreateDepositAccountRequest request);
        Task<bool> UpdateAsync(int id, CreateDepositAccountRequest request);
        Task<bool> SetActiveAsync(int id, bool isActive);
        Task<bool> DeleteAsync(int id);
    }
}