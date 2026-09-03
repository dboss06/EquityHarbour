namespace EquityHarbour.DTOs.WithdrawalLimits
{
    public class WithdrawalLimitTierDto
    {
        public int Id { get; set; }
        public decimal MinInvestedAmount { get; set; }
        public decimal? MaxInvestedAmount { get; set; }
        public decimal MinWithdrawalAmount { get; set; }
        public decimal MaxWithdrawalAmount { get; set; }
        public int MinReferralCount { get; set; }
        public bool IsActive { get; set; }
    }
}