namespace EquityHarbour.Models
{
    public class TaskClaim
    {
        public int Id { get; set; }
        public string UserId { get; set; } = string.Empty;
        public ApplicationUser User { get; set; } = null!;
        public int MilestoneTarget { get; set; }
        public decimal RewardAmount { get; set; }
        public DateTime ClaimedAt { get; set; } = DateTime.UtcNow;
    }
}