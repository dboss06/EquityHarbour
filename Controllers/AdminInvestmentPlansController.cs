using EquityHarbour.DTOs.InvestmentPlans;
using EquityHarbour.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EquityHarbour.Controllers
{

    [ApiController]
    [Route("api/admin/investment-plans")]
    [Authorize(Roles = "Admin")]
    public class AdminInvestmentPlansController : ControllerBase
    {
        private readonly IInvestmentPlanService _investmentPlanService;
        public AdminInvestmentPlansController(IInvestmentPlanService investmentPlanService)
        {
            _investmentPlanService = investmentPlanService;
        }
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var plans = await _investmentPlanService.GetAllAsync();
            return Ok(plans);
        }
        [HttpGet("{id:int}")]
        public async Task<IActionResult> GetById(int id)
        {
            var plan = await _investmentPlanService.GetByIdAsync(id);
            if (plan == null)
            {
                return NotFound(new
                {
                    message = "Investment plan not found."
                });
            }
            return Ok(plan);
        }
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateInvestmentPlanRequest request)
        {
            try
            {
                var plan = await _investmentPlanService.CreateAsync(request);
                return CreatedAtAction(nameof(GetById), new { id = plan.Id }, plan);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new
                {
                    message = ex.Message
                });
            }
            catch (InvalidOperationException ex)
            {
                return Conflict(new
                {
                    message = ex.Message
                });
            }
        }
        [HttpPut("{id:int}")]
        public async Task<IActionResult> Update(int id, [FromBody] UpdateInvestmentPlanRequest request)
        {
            try
            {
                var updated = await _investmentPlanService.UpdateAsync(id, request);

                if (!updated)
                {
                    return NotFound(new
                    {
                        message = "Investment plan not found."
                    });
                }
                return Ok(new
                {
                    message = "Investment plan updated successfully."
                });
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new
                {
                    message = ex.Message
                });
            }
            catch (InvalidOperationException ex)
            {
                return Conflict(new
                {
                    message = ex.Message
                });
            }
        }

        [HttpPatch("{id:int}/activate")]
        public async Task<IActionResult> Activate(int id)
        {
            var result = await _investmentPlanService.SetActiveAsync(id, true);
            if (!result)
            {
                return NotFound(new
                {
                    message = "Investment plan not found."
                });
            }
            return Ok(new
            {
                message = "Investment plan activated successfully."
            });
        }

        [HttpPatch("{id:int}/deactivate")]
        public async Task<IActionResult> Deactivate(int id)
        {
            var result = await _investmentPlanService.SetActiveAsync(id, false);
            if (!result)
            {
                return NotFound(new
                {
                    message = "Investment plan not found."
                });
            }
            return Ok(new
            {
                message = "Investment plan deactivated successfully."
            });
        }
    }
}