namespace EquityHarbour.Models;

public class Investment
{
    public long Id { get; set; }
    public string UserId { get; set; } = string.Empty;
    public ApplicationUser User { get; set; } = null!;
    public int InvestmentPlanId { get; set; }
    public InvestmentPlan InvestmentPlan { get; set; } = null!;
    public decimal PrincipalAmount { get; set; }
    public InvestmentReturnType ReturnType { get; set; }
    public decimal ReturnValue { get; set; }
    public decimal ExpectedReturn { get; set; }
    public int DurationDays { get; set; }
    public InvestmentPayoutFrequency PayoutFrequency { get; set; }
    public InvestmentStatus Status { get; set; } = InvestmentStatus.Pending;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? StartedAt { get; set; }
    public DateTime? MaturityDate { get; set; }
    public DateTime? CompletedAt { get; set; }
    public ICollection<InvestmentPayout> Payouts { get; set; } = new List<InvestmentPayout>();
    public uint RowVersion { get; set; }
}