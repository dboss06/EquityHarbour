namespace EquityHarbour.Models.ViewModels
{
    public class TaskMilestoneViewModel
    {
        public string Title { get; set; } = string.Empty;
        public decimal Reward { get; set; }
        public int CurrentStreak { get; set; }
        public int Target { get; set; }
        public int ProgressPercent => Target == 0 ? 0 : (int)((decimal)CurrentStreak / Target * 100);
        public string StatusLabel => ProgressPercent >= 100 ? "Completed" : "In Progress";
    }
}