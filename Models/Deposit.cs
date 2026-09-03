namespace EquityHarbour.Models { 

    public class Deposit
    {
        public long Id { get; set; }

        public decimal Amount { get; set; }

        public string Reference { get; set; } = string.Empty;

        public string? Description { get; set; }
        public string? AccountBankName { get; set; }
        public string? AccountNumber { get; set; }
        public string? AccountName { get; set; }
        public DepositStatus Status { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public DateTime? CompletedAt { get; set; }

        public string UserId { get; set; } = string.Empty;

        public ApplicationUser User { get; set; } = null!;

        public int WalletId { get; set; }

        public Wallet Wallet { get; set; } = null!;
        public string? ProofImagePath { get; set; }
        public string? UserProvidedReference { get; set; }
    }
}