using EquityHarbour.DTOs.WithdrawalLimits;

namespace EquityHarbour.Services
{
    public interface IWithdrawalLimitService
    {
        Task<List<WithdrawalLimitTierDto>> GetAllAsync();
        Task<WithdrawalLimitTierDto> CreateAsync(CreateWithdrawalLimitTierRequest request);
        Task<bool> SetActiveAsync(int id, bool isActive);
        Task<bool> DeleteAsync(int id);
        Task<WithdrawalLimitTierDto?> GetApplicableTierAsync(decimal totalInvested);
    }
}