using EquityHarbour.DTOs.Investments;
using EquityHarbour.Models;
using EquityHarbour.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace EquityHarbour.Controllers
{
    [Authorize]
    public class InvestmentsController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IInvestmentService _investmentService;

        public InvestmentsController(UserManager<ApplicationUser> userManager, IInvestmentService investmentService)
        {
            _userManager = userManager;
            _investmentService = investmentService;
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(int planId, decimal amount)
        {
            var user = await _userManager.GetUserAsync(User);

            var request = new CreateInvestmentRequest
            {
                InvestmentPlanId = planId,
                Amount = amount
            };

            try
            {
                await _investmentService.CreateAsync(user.Id, request);
                TempData["Success"] = "Investment successful!";
            }
            catch (Exception ex)
            {
                TempData["Error"] = ex.Message;
            }

            return RedirectToAction("Index", "Home");
        }
    }
}