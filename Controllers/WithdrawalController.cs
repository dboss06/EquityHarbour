using EquityHarbour.DTOs.Withdrawal;
using EquityHarbour.Models;
using EquityHarbour.Models.ViewModels;
using EquityHarbour.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace EquityHarbour.Controllers
{
    [Authorize]
    public class WithdrawController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IWalletService _walletService;
        private readonly IWithdrawalService _withdrawalService;
        private readonly IBankAccountService _bankAccountService;

        public WithdrawController(
            UserManager<ApplicationUser> userManager,
            IWalletService walletService,
            IWithdrawalService withdrawalService,
            IBankAccountService bankAccountService)
        {
            _userManager = userManager;
            _walletService = walletService;
            _withdrawalService = withdrawalService;
            _bankAccountService = bankAccountService;
        }

        public async Task<IActionResult> Index()
        {
            var user = await _userManager.GetUserAsync(User);
            var wallet = await _walletService.GetUserWalletAsync(user.Id);
            var savedAccount = await _bankAccountService.GetByUserIdAsync(user.Id);

            var model = new WithdrawViewModel
            {
                CurrentBalance = wallet?.AvailableBalance ?? 0,
                LockedBalance = wallet?.LockedBalance ?? 0
            };

            if (savedAccount != null)
            {
                model.BankName = savedAccount.BankName;
                model.AccountNumber = savedAccount.AccountNumber;
                model.AccountName = savedAccount.AccountName;
            }

            return View(model);
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(WithdrawViewModel model)
        {
            var user = await _userManager.GetUserAsync(User);
            var wallet = await _walletService.GetUserWalletAsync(user.Id);

            // Client-facing guard so an obviously-insufficient request doesn't
            // round-trip to the service's own exception — the service still
            // re-checks this itself as the source of truth.
            if (wallet != null && model.Amount > wallet.AvailableBalance)
            {
                ModelState.AddModelError(nameof(model.Amount), "Amount exceeds your available balance.");
            }

            if (!ModelState.IsValid)
            {
                model.CurrentBalance = wallet?.AvailableBalance ?? 0;
                model.LockedBalance = wallet?.LockedBalance ?? 0;
                return View("Index", model);
            }

            try
            {
                var withdrawal = await _withdrawalService.CreateAsync(user.Id, new CreateWithdrawalRequest
                {
                    Amount = model.Amount,
                    BankName = model.BankName,
                    AccountName = model.AccountName,
                    AccountNumber = model.AccountNumber,
                    Description = model.Description
                });

                return RedirectToAction("Pending", new { reference = withdrawal.Reference });
            }
            catch (Exception ex)
            {
                ModelState.AddModelError(string.Empty, ex.Message);
                model.CurrentBalance = wallet?.AvailableBalance ?? 0;
                model.LockedBalance = wallet?.LockedBalance ?? 0;
                return View("Index", model);
            }
        }

        public IActionResult Pending(string reference)
        {
            ViewData["Reference"] = reference;
            return View();
        }
    }
}