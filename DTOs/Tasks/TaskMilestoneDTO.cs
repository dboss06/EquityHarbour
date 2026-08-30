namespace EquityHarbour.DTOs.Tasks
{
    public class TaskMilestoneDto
    {
        public int Target { get; set; }
        public decimal RewardAmount { get; set; }
        public int CurrentCount { get; set; }
        public bool IsClaimed { get; set; }
        public int ProgressPercent => Target == 0 ? 0 : Math.Min(100, (int)((decimal)CurrentCount / Target * 100));
        public bool CanClaim => !IsClaimed && CurrentCount >= Target;
    }
}