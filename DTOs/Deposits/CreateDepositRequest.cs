namespace EquityHarbour.DTOs.Deposits
{

    public class CreateDepositRequest
    {
        public decimal Amount { get; set; }

        public string? Description { get; set; }
        public int? DepositAccountId { get; set; }
        public string? ProofImagePath { get; set; }
        public string? UserProvidedReference { get; set; }
    }
}