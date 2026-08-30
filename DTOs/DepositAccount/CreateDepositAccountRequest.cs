namespace EquityHarbour.DTOs.DepositAccounts
{
    public class CreateDepositAccountRequest
    {
        public string BankName { get; set; } = string.Empty;
        public string AccountNumber { get; set; } = string.Empty;
        public string AccountName { get; set; } = string.Empty;
        public int DisplayOrder { get; set; }
    }
}