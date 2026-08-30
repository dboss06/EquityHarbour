using EquityHarbour.DTOs.Investments;

namespace EquityHarbour.Services;

public interface IInvestmentService
{
    Task<InvestmentDto> CreateAsync(string userId, CreateInvestmentRequest request);
    Task<List<InvestmentDto>> GetAllAsync();
    Task<List<InvestmentDto>> GetUserInvestmentsAsync(string userId);

    Task<InvestmentDto?> GetUserInvestmentAsync(string userId, long id);
    Task<InvestmentDto> ProcessMaturityAsync(long investmentId);
    Task<List<InvestmentDto>> ProcessMaturedInvestmentsAsync();
    Task<InvestmentDto> PrepareTestPayoutPeriodAsync(long investmentId);
}