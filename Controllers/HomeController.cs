using EquityHarbour.Models;
using EquityHarbour.Models.ViewModels;
using EquityHarbour.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace EquityHarbour.Controllers
{
    [Authorize]
    public class HomeController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IWalletService _walletService;
        private readonly IInvestmentPlanService _planService;

        public HomeController(
            UserManager<ApplicationUser> userManager,
            IWalletService walletService,
            IInvestmentPlanService planService)
        {
            _userManager = userManager;
            _walletService = walletService;
            _planService = planService;
        }

        public async Task<IActionResult> Index()
        {
            var user = await _userManager.GetUserAsync(User);
            var wallet = await _walletService.GetUserWalletAsync(user.Id);
            var plans = await _planService.GetActiveAsync();

            if (wallet == null)
            {
                // Shouldn't happen since AuthService creates one at registration,
                // but guards against a null-ref if it's ever missing.
                TempData["Error"] = "Wallet not found for this account.";
                return View(new DashboardViewModel { UserEmail = user.Email ?? "" });
            }

            var model = new DashboardViewModel
            {
                UserEmail = user.Email ?? "",
                AvailableBalance = wallet.AvailableBalance,
                InvestedBalance = wallet.InvestedBalance,
                TotalDeposited = wallet.TotalDeposited,
                LockedBalance = wallet.LockedBalance,
                Plans = plans.Select(p => new InvestmentPlanViewModel { 
                    Id = p.Id,
                    Name = p.Name,
                    DurationDays = p.DurationDays,
                    MinimumAmount = p.MinimumAmount,
                    MaximumAmount = p.MaximumAmount,
                    ReturnValue = p.ReturnValue
                }).ToList()
            };

            return View(model);
        }
    }
}