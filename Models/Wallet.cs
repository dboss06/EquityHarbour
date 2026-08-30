namespace EquityHarbour.Models
{
    public class Wallet
    {
        public int Id { get; set; }
        public decimal AvailableBalance { get; set; }
        public decimal InvestedBalance { get; set; }
        public decimal TotalDeposited { get; set; }
        public decimal TotalWithdrawn { get; set; }
        public decimal TotalProfit { get; set; }
        public string UserId { get; set; } = string.Empty;
        public ApplicationUser User { get; set; } = null!;
        public ICollection<WalletTransaction> Transactions { get; set; } = new List<WalletTransaction>();
        public uint RowVersion { get; set; }
    }
}
