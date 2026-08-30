using EquityHarbour.DTOs.Tasks;

namespace EquityHarbour.Services
{
    public interface ITaskService
    {
        Task<List<TaskMilestoneDto>> GetMilestonesAsync(string userId);
        Task<decimal> ClaimAsync(string userId, int target);
    }
}