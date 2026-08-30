namespace EquityHarbour.Areas.Admin.Models
{
    public class DashboardViewModel
    {
        public int TotalUsers { get; set; }
        public int ActiveInvestments { get; set; }
        public int MaturedInvestments { get; set; }
        public int CancelledInvestments { get; set; }
        public int PendingDeposits { get; set; }
        public int PendingWithdrawals { get; set; }
        public decimal TotalDeposits { get; set; }
        public decimal TotalWithdrawals { get; set; }
        public decimal TotalInvested { get; set; }

        public List<RecentUserItem> RecentUsers { get; set; } = new();
        public List<RecentActivityItem> RecentActivity { get; set; } = new();
    }

    public class RecentUserItem
    {
        public string Id { get; set; } = string.Empty;
        public string FullName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
    }

    public class RecentActivityItem
    {
        public string Reference { get; set; } = string.Empty;
        public string Type { get; set; } = string.Empty; // "Deposit" or "Withdrawal"
        public decimal Amount { get; set; }
        public string Status { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
    }
}