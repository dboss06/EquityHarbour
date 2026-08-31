namespace EquityHarbour.DTOs.WithdrawalLimits
{
    public class CreateWithdrawalLimitTierRequest
    {
        public decimal MinInvestedAmount { get; set; }
        public decimal? MaxInvestedAmount { get; set; }
        public decimal MinWithdrawalAmount { get; set; }
        public decimal MaxWithdrawalAmount { get; set; }
    }
}