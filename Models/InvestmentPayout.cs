namespace EquityHarbour.Models
{
    public class InvestmentPayout
    {
        public long Id { get; set; }
        public long InvestmentId { get; set; }
        public Investment Investment { get; set; } = null!;
        public decimal Amount { get; set; }
        public InvestmentPayoutFrequency Frequency { get; set; }
        public DateTime PeriodStart { get; set; }
        public DateTime PeriodEnd { get; set; }
        public DateTime PaidAt { get; set; } = DateTime.UtcNow;
    }
}
