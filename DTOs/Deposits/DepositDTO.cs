using EquityHarbour.Models;

namespace EquityHarbour.DTOs.Deposits { 

    public class DepositDTO
    {
        public long Id { get; set; }

        public decimal Amount { get; set; }
        public string UserId { get; set; } = string.Empty;
        public string UserFullName { get; set; } = string.Empty;
        public string Reference { get; set; } = string.Empty;

        public string? Description { get; set; }

        public DepositStatus Status { get; set; }

        public DateTime CreatedAt { get; set; }

        public DateTime? CompletedAt { get; set; }
        public string? AccountBankName { get; set; }
        public string? AccountNumber { get; set; }
        public string? AccountName { get; set; }
    }
}