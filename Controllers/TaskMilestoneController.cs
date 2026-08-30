using EquityHarbour.Models;
using EquityHarbour.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace EquityHarbour.Controllers
{
    [Authorize]
    public class TasksController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ITaskService _taskService;

        public TasksController(UserManager<ApplicationUser> userManager, ITaskService taskService)
        {
            _userManager = userManager;
            _taskService = taskService;
        }

        public async Task<IActionResult> Index()
        {
            var user = await _userManager.GetUserAsync(User);
            var milestones = await _taskService.GetMilestonesAsync(user.Id);
            return View(milestones);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Claim(int target)
        {
            var user = await _userManager.GetUserAsync(User);

            try
            {
                var reward = await _taskService.ClaimAsync(user.Id, target);
                TempData["Success"] = $"₦{reward:N2} claimed and credited to your wallet!";
            }
            catch (Exception ex)
            {
                TempData["Error"] = ex.Message;
            }

            return RedirectToAction("Index");
        }
    }
}