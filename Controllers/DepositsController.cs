using EquityHarbour.DTOs.Deposits;
using EquityHarbour.Models;
using EquityHarbour.Models.ViewModels;
using EquityHarbour.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace EquityHarbour.Controllers
{
    [Authorize]
    public class DepositController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IWalletService _walletService;
        private readonly IDepositService _depositService;
        private readonly IDepositAccountService _accountService;
        

        public DepositController(
            UserManager<ApplicationUser> userManager,
            IWalletService walletService,
            IDepositService depositService,
            IDepositAccountService accountService)
        {
            _userManager = userManager;
            _walletService = walletService;
            _depositService = depositService;
            _accountService = accountService;
        }

        public async Task<IActionResult> Index()
        {
            var user = await _userManager.GetUserAsync(User);
            var wallet = await _walletService.GetUserWalletAsync(user.Id);
            var accounts = await _accountService.GetActiveAsync();

            return View(new DepositViewModel
            {
                CurrentBalance = wallet?.AvailableBalance ?? 0,
                Accounts = accounts
            });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(DepositViewModel model, int? accountId)
        {
            var user = await _userManager.GetUserAsync(User);

            if (!ModelState.IsValid)
            {
                var wallet = await _walletService.GetUserWalletAsync(user.Id);
                model.CurrentBalance = wallet?.AvailableBalance ?? 0;
                model.Accounts = await _accountService.GetActiveAsync();
                return View("Index", model);
            }

            try
            {
                var deposit = await _depositService.CreateAsync(user.Id, new CreateDepositRequest
                {
                    Amount = model.Amount,
                    Description = model.Description,
                    DepositAccountId = accountId
                });

                return RedirectToAction("Pending", new
                {
                    reference = deposit.Reference,
                    bank = deposit.AccountBankName,
                    number = deposit.AccountNumber,
                    name = deposit.AccountName
                });
            }
            catch (Exception ex)
            {
                ModelState.AddModelError(string.Empty, ex.Message);
                var wallet = await _walletService.GetUserWalletAsync(user.Id);
                model.CurrentBalance = wallet?.AvailableBalance ?? 0;
                model.Accounts = await _accountService.GetActiveAsync();
                return View("Index", model);
            }
        }

        public IActionResult Pending(string reference, string? bank, string? number, string? name)
        {
            ViewData["Reference"] = reference;
            ViewData["Bank"] = bank;
            ViewData["Number"] = number;
            ViewData["Name"] = name;
            return View();
        }
    }
    }
