using EquityHarbour.DTOs.BankAccounts;
using EquityHarbour.Models;
using EquityHarbour.Models.ViewModels;
using EquityHarbour.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace EquityHarbour.Controllers
{
    [Authorize]
    public class BankController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IBankAccountService _bankAccountService;

        public BankController(UserManager<ApplicationUser> userManager, IBankAccountService bankAccountService)
        {
            _userManager = userManager;
            _bankAccountService = bankAccountService;
        }

        public async Task<IActionResult> Index()
        {
            var user = await _userManager.GetUserAsync(User);
            var existing = await _bankAccountService.GetByUserIdAsync(user.Id);

            var model = new BankAccountViewModel();
            if (existing != null)
            {
                model.BankName = existing.BankName;
                model.AccountNumber = existing.AccountNumber;
                model.AccountName = existing.AccountName;
            }

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Add(BankAccountViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View("Index", model);
            }

            var user = await _userManager.GetUserAsync(User);

            try
            {
                await _bankAccountService.UpsertAsync(user.Id, new SaveBankAccountRequest
                {
                    BankName = model.BankName,
                    AccountNumber = model.AccountNumber,
                    AccountName = model.AccountName
                });

                TempData["Success"] = "Bank account saved successfully.";
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                ModelState.AddModelError(string.Empty, ex.Message);
                return View("Index", model);
            }
        }
    }
}