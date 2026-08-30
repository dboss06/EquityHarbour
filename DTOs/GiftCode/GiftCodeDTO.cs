namespace EquityHarbour.DTOs.GiftCodes
{
    public class GiftCodeDto
    {
        public int Id { get; set; }
        public string Code { get; set; } = string.Empty;
        public decimal Amount { get; set; }
        public bool IsRedeemed { get; set; }
        public string? RedeemedByUserFullName { get; set; }
        public DateTime? RedeemedAt { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}