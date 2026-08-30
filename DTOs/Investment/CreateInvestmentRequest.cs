namespace EquityHarbour.DTOs.Investments
{

    public class CreateInvestmentRequest
    {
        public int InvestmentPlanId { get; set; }
        public decimal Amount { get; set; }
    }
}