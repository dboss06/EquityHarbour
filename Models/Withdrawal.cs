using EquityHarbour.Models.Enums;

namespace EquityHarbour.Models
{
    public class Withdrawal
    {
        public long Id { get; set; }
        public decimal Amount { get; set; }
        public string Reference { get; set; } = string.Empty;
        public string BankName { get; set; } = string.Empty;
        public string AccountName { get; set; } = string.Empty;
        public string AccountNumber { get; set; } = string.Empty;
        public WithdrawalStatus Status { get; set; }
        public string? Description { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? CompletedAt { get; set; }
        public string UserId { get; set; } = string.Empty;
        public ApplicationUser User { get; set; } = null!;
        public int WalletId { get; set; }
        public Wallet Wallet { get; set; } = null!;
    }
}
