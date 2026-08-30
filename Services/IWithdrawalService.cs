using EquityHarbour.DTOs.Withdrawal;

namespace EquityHarbour.Services
{
    public interface IWithdrawalService
    {
        Task<WithdrawalDto> CreateAsync(string userId, CreateWithdrawalRequest request);
        Task<List<WithdrawalDto>> GetUserWithdrawalsAsync(string userId);
        Task<WithdrawalDto?> GetUserWithdrawalAsync(string userId, long id);
        Task<List<WithdrawalDto>> GetAllAsync();
        Task<WithdrawalDto?> GetByIdAsync(long id);
        Task<WithdrawalDto> ApproveAsync(long id);
        Task<WithdrawalDto> RejectAsync(long id);
    }
}
