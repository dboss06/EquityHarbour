namespace EquityHarbour.Models
{
    public class ReferralCommission
    {
        public long Id { get; set; }
        public string ReferrerId { get; set; } = string.Empty;
        public ApplicationUser Referrer { get; set; } = null!;
        public string SourceUserId { get; set; } = string.Empty;
        public ApplicationUser SourceUser { get; set; } = null!;
        public int Level { get; set; }
        public decimal SourceAmount { get; set; }
        public decimal CommissionAmount { get; set; }
        public string SourceType { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}