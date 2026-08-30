using EquityHarbour.Models;

namespace EquityHarbour.DTOs.InvestmentPlans;

public class CreateInvestmentPlanRequest
{
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public decimal MinimumAmount { get; set; }
    public decimal MaximumAmount { get; set; }
    public int DurationDays { get; set; }
    public InvestmentReturnType ReturnType { get; set; }
    public decimal ReturnValue { get; set; }
    public InvestmentPayoutFrequency PayoutFrequency { get; set; }
    public bool IsActive { get; set; } = true;
}