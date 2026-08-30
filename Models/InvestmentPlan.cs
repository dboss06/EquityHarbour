namespace EquityHarbour.Models
{
    public class InvestmentPlan
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public decimal MinimumAmount { get; set; }
        public decimal MaximumAmount { get; set; }
        public int DurationDays { get; set; }
        public InvestmentReturnType ReturnType { get; set; }
        public decimal ReturnValue { get; set; }
        public InvestmentPayoutFrequency PayoutFrequency { get; set; }
        public bool IsActive { get; set; } = true;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; }
        public ICollection<Investment> Investments { get; set; } = new List<Investment>();
    }
}
