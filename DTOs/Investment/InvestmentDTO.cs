using EquityHarbour.Models;

namespace EquityHarbour.DTOs.Investments
{

    public class InvestmentDto
    {
        public long Id { get; set; }
        public string UserId { get; set; } = string.Empty;
        public string UserFullName { get; set; } = string.Empty;
        public int InvestmentPlanId { get; set; }
        public string InvestmentPlanName { get; set; } = string.Empty;
        public decimal PrincipalAmount { get; set; }
        public InvestmentReturnType ReturnType { get; set; }
        public decimal ReturnValue { get; set; }
        public decimal ExpectedReturn { get; set; }
        public int DurationDays { get; set; }
        public InvestmentPayoutFrequency PayoutFrequency { get; set; }
        public InvestmentStatus Status { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? StartedAt { get; set; }
        public DateTime? MaturityDate { get; set; }
        public DateTime? CompletedAt { get; set; }
    }
}