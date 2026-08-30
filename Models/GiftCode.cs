namespace EquityHarbour.Models
{
    public class GiftCode
    {
        public int Id { get; set; }
        public string Code { get; set; } = string.Empty;
        public decimal Amount { get; set; }
        public bool IsRedeemed { get; set; }
        public string? RedeemedByUserId { get; set; }
        public ApplicationUser? RedeemedByUser { get; set; }
        public DateTime? RedeemedAt { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}