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
        private readonly IInvestmentService _investmentService;
        private readonly IWithdrawalLimitService _limitService;
        private readonly IReferralService _referralService;

        public WithdrawController(
            UserManager<ApplicationUser> userManager,
            IWalletService walletService,
            IWithdrawalService withdrawalService,
            IInvestmentService investmentService,
            IWithdrawalLimitService limitService,
            IReferralService referralService,
            IBankAccountService bankAccountService)
        {
            _userManager = userManager;
            _walletService = walletService;
            _withdrawalService = withdrawalService;
            _investmentService = investmentService;
            _limitService = limitService;
            _referralService = referralService;
            _bankAccountService = bankAccountService;
        }

        private async Task PopulateTierInfoAsync(WithdrawViewModel model, string userId)
        {
            var investments = await _investmentService.GetUserInvestmentsAsync(userId);
            var totalInvested = investments.Sum(i => i.PrincipalAmount);
            var tier = await _limitService.GetApplicableTierAsync(totalInvested);
            var qualifiedReferrals = await _referralService.GetQualifiedReferralCountAsync(userId);

            model.HasApplicableTier = tier != null;
            model.RequiredReferralCount = tier?.MinReferralCount ?? 0;
            model.QualifiedReferralCount = qualifiedReferrals;
            model.TierMinWithdrawal = tier?.MinWithdrawalAmount;
            model.TierMaxWithdrawal = tier?.MaxWithdrawalAmount;
        }  
        public async Task<IActionResult> Index()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return RedirectToAction("Login", "Account");
            }

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
            await PopulateTierInfoAsync(model, user.Id);
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