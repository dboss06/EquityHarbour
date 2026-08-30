using EquityHarbour.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EquityHarbour.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin")]
    public class WithdrawalsController : Controller
    {
        private readonly IWithdrawalService _withdrawalService;

        public WithdrawalsController(IWithdrawalService withdrawalService)
        {
            _withdrawalService = withdrawalService;
        }

        public async Task<IActionResult> Index()
        {
            ViewData["Title"] = "Withdrawals";
            ViewData["PageTitle"] = "Withdrawals";
            var withdrawals = await _withdrawalService.GetAllAsync();
            return View(withdrawals);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Approve(long id)
        {
            try
            {
                await _withdrawalService.ApproveAsync(id);
                TempData["Success"] = "Withdrawal approved successfully.";
            }
            catch (Exception ex)
            {
                TempData["Error"] = ex.Message;
            }
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Reject(long id)
        {
            try
            {
                await _withdrawalService.RejectAsync(id);
                TempData["Success"] = "Withdrawal rejected — funds returned to user's wallet.";
            }
            catch (Exception ex)
            {
                TempData["Error"] = ex.Message;
            }
            return RedirectToAction(nameof(Index));
        }
    }
}