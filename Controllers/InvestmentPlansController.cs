using EquityHarbour.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EquityHarbour.Controllers
{
    [ApiController]
    [Route("api/investment-plans")]
    [Authorize]
    public class InvestmentPlansController : ControllerBase
    {
        private readonly IInvestmentPlanService _investmentPlanService;

        public InvestmentPlansController(IInvestmentPlanService investmentPlanService)
        {
            _investmentPlanService = investmentPlanService;
        }
        [HttpGet]
        public async Task<IActionResult> GetActivePlans() {
            var plans = await _investmentPlanService.GetActiveAsync();
            return Ok(plans);
        }

        [HttpGet("{id:int}")]
        public async Task<IActionResult> GetById(int id) {
            var plan = await _investmentPlanService.GetByIdAsync(id);
            if(plan == null || !plan.IsActive)
            {
                return NotFound(new
                {
                    message = "Investment Plan not found"
                });
            }
            return Ok(plan);
        }
    }
}
