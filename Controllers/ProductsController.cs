using EquityHarbour.Models;
using EquityHarbour.Models.ViewModels;
using EquityHarbour.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace EquityHarbour.Controllers
{
    [Authorize]
    public class ProductsController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IInvestmentService _investmentService;

        public ProductsController(UserManager<ApplicationUser> userManager, IInvestmentService investmentService)
        {
            _userManager = userManager;
            _investmentService = investmentService;
        }

        public async Task<IActionResult> Index()
        {
            var user = await _userManager.GetUserAsync(User);
            var investments = await _investmentService.GetUserInvestmentsAsync(user.Id);

            var totalDailyIncome = investments
                .Where(i => i.Status == InvestmentStatus.Active && i.PayoutFrequency == InvestmentPayoutFrequency.Daily)
                .Sum(i => i.ExpectedReturn / i.DurationDays);

            var model = new ProductsViewModel
            {
                TotalDailyIncome = totalDailyIncome,
                Investments = investments
            };

            return View(model);
        }
    }
}