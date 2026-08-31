namespace EquityHarbour.Models
{
    public class WithdrawalLimitTier
    {
        public int Id { get; set; }
        public decimal MinInvestedAmount { get; set; }
        public decimal? MaxInvestedAmount { get; set; }
        public decimal MinWithdrawalAmount { get; set; }
        public decimal MaxWithdrawalAmount { get; set; }
        public bool IsActive { get; set; } = true;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}