using EquityHarbour.DTOs.DepositAccounts;
using EquityHarbour.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EquityHarbour.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin")]
    public class DepositAccountsController : Controller
    {
        private readonly IDepositAccountService _accountService;

        public DepositAccountsController(IDepositAccountService accountService)
        {
            _accountService = accountService;
        }

        public async Task<IActionResult> Index()
        {
            ViewData["Title"] = "Deposit Accounts";
            ViewData["PageTitle"] = "Deposit Accounts";
            var accounts = await _accountService.GetAllAsync();
            return View(accounts);
        }

        public IActionResult Create()
        {
            ViewData["Title"] = "Add Deposit Account";
            ViewData["PageTitle"] = "Add Deposit Account";
            return View(new CreateDepositAccountRequest());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CreateDepositAccountRequest request)
        {
            try
            {
                await _accountService.CreateAsync(request);
                TempData["Success"] = "Deposit account added.";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                ModelState.AddModelError(string.Empty, ex.Message);
                ViewData["Title"] = "Add Deposit Account";
                ViewData["PageTitle"] = "Add Deposit Account";
                return View(request);
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ToggleActive(int id)
        {
            var accounts = await _accountService.GetAllAsync();
            var account = accounts.FirstOrDefault(a => a.Id == id);
            if (account == null) return NotFound();

            await _accountService.SetActiveAsync(id, !account.IsActive);
            TempData["Success"] = "Account status updated.";
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var deleted = await _accountService.DeleteAsync(id);
            TempData[deleted ? "Success" : "Error"] = deleted ? "Account deleted." : "Account not found.";
            return RedirectToAction(nameof(Index));
        }
    }
}