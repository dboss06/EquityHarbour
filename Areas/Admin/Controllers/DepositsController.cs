using EquityHarbour.Models;
using EquityHarbour.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EquityHarbour.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin")]
    public class DepositsController : Controller
    {
        private readonly IDepositService _depositService;

        public DepositsController(IDepositService depositService)
        {
            _depositService = depositService;
        }

        public async Task<IActionResult> Index()
        {
            ViewData["Title"] = "Deposits";
            ViewData["PageTitle"] = "Deposits";
            var deposits = await _depositService.GetAllAsync();
            return View(deposits);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Approve(long id)
        {
            try
            {
                await _depositService.ApproveAsync(id);
                TempData["Success"] = "Deposit approved successfully.";
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
                await _depositService.RejectAsync(id);
                TempData["Success"] = "Deposit rejected.";
            }
            catch (Exception ex)
            {
                TempData["Error"] = ex.Message;
            }
            return RedirectToAction(nameof(Index));
        }
    }
}