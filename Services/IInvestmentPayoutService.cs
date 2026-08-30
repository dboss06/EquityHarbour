namespace EquityHarbour.Services
{
    public interface IInvestmentPayoutService
    {
        Task<int> ProcessPendingPayoutsAsync(long investmentId);
        Task<int> ProcessAllPendingPayoutsAsync();
    }
}
