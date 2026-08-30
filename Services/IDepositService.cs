using EquityHarbour.DTOs.Deposits;

namespace EquityHarbour.Services
{
    public interface IDepositService
    {
        Task<DepositDTO> CreateAsync(string userId, CreateDepositRequest request);
        Task<List<DepositDTO>> GetUserDepositsAsync(string UserId);
        Task<DepositDTO?> GetUserDepositAsync(string userId, long id);
        Task<List<DepositDTO>> GetAllAsync();
        Task<DepositDTO?> GetByIdAsync(long id);
        Task<DepositDTO> ApproveAsync(long id);
        Task<DepositDTO> RejectAsync(long id);
    }
}
