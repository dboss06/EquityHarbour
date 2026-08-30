using EquityHarbour.DTOs.InvestmentPlans;

namespace EquityHarbour.Services
{
    public interface IInvestmentPlanService
    {
        Task<List<InvestmentPlanDto>> GetAllAsync();
        Task<List<InvestmentPlanDto>> GetActiveAsync();
        Task<InvestmentPlanDto?> GetByIdAsync(int id);
        Task<InvestmentPlanDto> CreateAsync(CreateInvestmentPlanRequest request);
        Task<bool> UpdateAsync(int id, UpdateInvestmentPlanRequest request);
        Task<bool> SetActiveAsync(int id, bool isActive);
        Task<bool> DeleteAsync(int id);
    }
}
