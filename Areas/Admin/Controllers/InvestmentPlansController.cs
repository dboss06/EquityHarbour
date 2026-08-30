using EquityHarbour.DTOs.InvestmentPlans;
using EquityHarbour.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EquityHarbour.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin")]
    public class InvestmentPlansController : Controller
    {
        private readonly IInvestmentPlanService _planService;
        private readonly IInvestmentService _investmentService;

        public InvestmentPlansController(IInvestmentPlanService planService, IInvestmentService investmentService)
        {
            _planService = planService;
            _investmentService = investmentService;
        }

        public async Task<IActionResult> Index()
        {
            ViewData["Title"] = "Investment Plans";
            ViewData["PageTitle"] = "Investment Plans";

            var plans = await _planService.GetAllAsync();
            var investments = await _investmentService.GetAllAsync();

            ViewBag.TotalInvestors = investments.Select(i => i.UserId).Distinct().Count();
            ViewBag.TotalInvested = investments.Sum(i => i.PrincipalAmount);
            ViewBag.InvestorCounts = investments
                .GroupBy(i => i.InvestmentPlanId)
                .ToDictionary(g => g.Key, g => g.Count());

            return View(plans);
        }

        public IActionResult Create()
        {
            ViewData["Title"] = "Create Investment Plan";
            ViewData["PageTitle"] = "Create Investment Plan";
            return View(new CreateInvestmentPlanRequest());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CreateInvestmentPlanRequest request)
        {
            try
            {
                await _planService.CreateAsync(request);
                TempData["Success"] = "Plan created successfully.";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                ModelState.AddModelError(string.Empty, ex.Message);
                ViewData["Title"] = "Create Investment Plan";
                ViewData["PageTitle"] = "Create Investment Plan";
                return View(request);
            }
        }

        public async Task<IActionResult> Edit(int id)
        {
            ViewData["Title"] = "Edit Investment Plan";
            ViewData["PageTitle"] = "Edit Investment Plan";

            var plan = await _planService.GetByIdAsync(id);
            if (plan == null) return NotFound();

            var request = new UpdateInvestmentPlanRequest
            {
                Name = plan.Name,
                Description = plan.Description,
                MinimumAmount = plan.MinimumAmount,
                MaximumAmount = plan.MaximumAmount,
                DurationDays = plan.DurationDays,
                ReturnType = plan.ReturnType,
                ReturnValue = plan.ReturnValue,
                PayoutFrequency = plan.PayoutFrequency
            };

            ViewBag.PlanId = id;
            return View(request);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, UpdateInvestmentPlanRequest request)
        {
            ViewBag.PlanId = id;
            ViewData["Title"] = "Edit Investment Plan";
            ViewData["PageTitle"] = "Edit Investment Plan";

            try
            {
                var updated = await _planService.UpdateAsync(id, request);
                if (!updated) return NotFound();

                TempData["Success"] = "Investment plan updated successfully.";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                ModelState.AddModelError(string.Empty, ex.Message);
                return View(request);
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ToggleStatus(int id)
        {
            var plan = await _planService.GetByIdAsync(id);
            if (plan == null) return NotFound();

            await _planService.SetActiveAsync(id, !plan.IsActive);
            TempData["Success"] = "Plan status updated successfully.";
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                var deleted = await _planService.DeleteAsync(id);
                TempData[deleted ? "Success" : "Error"] = deleted
                    ? "Investment plan deleted successfully."
                    : "Plan not found.";
            }
            catch (Exception ex)
            {
                TempData["Error"] = ex.Message;
            }
            return RedirectToAction(nameof(Index));
        }
    }
}