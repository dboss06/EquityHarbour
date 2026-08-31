namespace EquityHarbour.Models.ViewModels
{
    public class DashboardViewModel
    {
        public string UserEmail { get; set; } = string.Empty;
        public decimal AvailableBalance { get; set; }
        public decimal InvestedBalance { get; set; }
        public decimal TotalDeposited { get; set; }
        public decimal TotalBalance => AvailableBalance + InvestedBalance;
        public decimal LockedBalance { get; set; }
        public List<InvestmentPlanViewModel> Plans { get; set; } = new();
    }

    public class InvestmentPlanViewModel
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public int DurationDays { get; set; }
        public decimal MinimumAmount { get; set; }
        public decimal MaximumAmount { get; set; }
        public decimal ReturnValue { get; set; }
    }
}