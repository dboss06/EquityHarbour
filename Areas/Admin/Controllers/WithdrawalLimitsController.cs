using EquityHarbour.DTOs.WithdrawalLimits;
using EquityHarbour.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EquityHarbour.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin")]
    public class WithdrawalLimitsController : Controller
    {
        private readonly IWithdrawalLimitService _limitService;

        public WithdrawalLimitsController(IWithdrawalLimitService limitService)
        {
            _limitService = limitService;
        }

        public async Task<IActionResult> Index()
        {
            ViewData["Title"] = "Withdrawal Limits";
            ViewData["PageTitle"] = "Withdrawal Limits";
            var tiers = await _limitService.GetAllAsync();
            return View(tiers);
        }

        public IActionResult Create()
        {
            ViewData["Title"] = "Add Withdrawal Tier";
            ViewData["PageTitle"] = "Add Withdrawal Tier";
            return View(new CreateWithdrawalLimitTierRequest());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CreateWithdrawalLimitTierRequest request)
        {
            try
            {
                await _limitService.CreateAsync(request);
                TempData["Success"] = "Withdrawal tier created.";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                ModelState.AddModelError(string.Empty, ex.Message);
                ViewData["Title"] = "Add Withdrawal Tier";
                ViewData["PageTitle"] = "Add Withdrawal Tier";
                return View(request);
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ToggleActive(int id)
        {
            var tiers = await _limitService.GetAllAsync();
            var tier = tiers.FirstOrDefault(t => t.Id == id);
            if (tier == null) return NotFound();

            await _limitService.SetActiveAsync(id, !tier.IsActive);
            TempData["Success"] = "Tier status updated.";
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var deleted = await _limitService.DeleteAsync(id);
            TempData[deleted ? "Success" : "Error"] = deleted ? "Tier deleted." : "Tier not found.";
            return RedirectToAction(nameof(Index));
        }
    }
}