using EquityHarbour.Areas.Admin.Models;
using EquityHarbour.Models;
using EquityHarbour.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace EquityHarbour.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin")]
    public class UsersController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IWalletService _walletService;
        private readonly IInvestmentService _investmentService;
        private readonly IReferralService _referralService;

        public UsersController(
            UserManager<ApplicationUser> userManager,
            IWalletService walletService,
            IInvestmentService investmentService,
            IReferralService referralService)
        {
            _userManager = userManager;
            _walletService = walletService;
            _investmentService = investmentService;
            _referralService = referralService;
        }

        public async Task<IActionResult> Index()
        {
            ViewData["Title"] = "Users";
            ViewData["PageTitle"] = "Users";

            var users = await _userManager.GetUsersInRoleAsync("User");
            var allInvestments = await _investmentService.GetAllAsync();

            var model = new List<UserListItemViewModel>();
            foreach (var user in users)
            {
                // TODO: N+1 here (wallet + transactions per user) — fine for
                // current data volume, but worth a bulk-query pass later if
                // the user list grows large.
                var wallet = await _walletService.GetUserWalletAsync(user.Id);
                var transactions = await _walletService.GetTransactionsAsync(user.Id);

                model.Add(new UserListItemViewModel
                {
                    Id = user.Id,
                    FullName = user.FullName,
                    Email = user.Email ?? "",
                    IsActive = user.IsActive,
                    WalletBalance = wallet?.AvailableBalance ?? 0,
                    InvestmentCount = allInvestments.Count(i => i.UserId == user.Id),// see note below
                    TransactionCount = transactions.Count
                });
            }

            return View(model.OrderByDescending(u => u.FullName).ToList());
        }

        public async Task<IActionResult> Details(string id)
        {
            ViewData["Title"] = "User Details";
            ViewData["PageTitle"] = "User Details";

            if (string.IsNullOrEmpty(id)) return NotFound();

            var user = await _userManager.FindByIdAsync(id);
            if (user == null) return NotFound();

            var wallet = await _walletService.GetUserWalletAsync(user.Id);
            var investments = await _investmentService.GetUserInvestmentsAsync(user.Id);
            var transactions = await _walletService.GetTransactionsAsync(user.Id);
            var directReferrals = await _referralService.GetDirectReferralsAsync(user.Id);
            var secondLevelReferrals = await _referralService.GetSecondLevelReferralsAsync(user.Id);
            var thirdLevelReferrals = await _referralService.GetThirdLevelReferralsAsync(user.Id);
            ApplicationUser? referredBy = null;
            if (!string.IsNullOrEmpty(user.ReferredByUserId))
            {
                referredBy = await _userManager.FindByIdAsync(user.ReferredByUserId);
            }
            var model = new UserDetailsViewModel
            {
                Id = user.Id,
                FullName = user.FullName,
                Email = user.Email ?? "",
                UserName = user.UserName ?? "",
                IsActive = user.IsActive,
                WalletBalance = wallet?.AvailableBalance ?? 0,
                Investments = investments,
                Transactions = transactions,
                DirectReferrals = directReferrals,
                SecondLevelReferrals = secondLevelReferrals,
                ThirdLevelReferrals = thirdLevelReferrals,
                ReferredById = referredBy?.Id,
                ReferredByFullName = referredBy?.FullName,
                ReferredByEmail = referredBy?.Email
            };
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ToggleActive(string id)
        {
            var user = await _userManager.FindByIdAsync(id);
            if (user == null) return NotFound();

            user.IsActive = !user.IsActive;
            user.DeactivatedAt = user.IsActive ? null : DateTime.UtcNow;

            var result = await _userManager.UpdateAsync(user);
            if (!result.Succeeded)
            {
                TempData["Error"] = string.Join("<br>", result.Errors.Select(e => e.Description));
                return RedirectToAction(nameof(Index));
            }

            TempData["Success"] = user.IsActive ? "User reactivated." : "User deactivated.";
            return RedirectToAction(nameof(Index));
        }
    }
}