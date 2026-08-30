namespace EquityHarbour.DTOs.Withdrawal
{
    public class CreateWithdrawalRequest
    {
        public decimal Amount { get; set; }
        public string BankName { get; set; } = string.Empty;
        public string AccountName { get; set; } = string.Empty;
        public string AccountNumber { get; set; } = string.Empty;
        public string? Description { get; set; }
    }
}
