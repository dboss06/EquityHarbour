using EquityHarbour.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EquityHarbour.Controllers
{
    [ApiController]
    [Route("api/admin/investments")]
    [Authorize(Roles = "Admin")]
    public class AdminInvestmentController : ControllerBase
    {
        private readonly IInvestmentService _investmentService;
        private readonly IInvestmentPayoutService _payoutService;

        public AdminInvestmentController(IInvestmentService investmentService, IInvestmentPayoutService payoutService)
        {
            _investmentService = investmentService;
            _payoutService = payoutService;
        }

        [HttpPost("{id:long}/process-maturity")]
        public async Task<IActionResult> ProcessMaturity(long id)
        {
            try
            {
                var investment = await _investmentService.ProcessMaturityAsync(id);
                return Ok(investment);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new
                {
                    message = ex.Message
                });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new
                {
                    message = ex.Message
                });
            }
        }
        [HttpPost("{id:long}/process-payouts")]
        public async Task<IActionResult> ProcessPayouts(long id)
        {
            try
            {
                var count = await _payoutService.ProcessPendingPayoutsAsync(id);
                return Ok(new
                {
                    investmentId = id,
                    payoutsProcessed = count
                });
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new
                {
                    message = ex.Message
                });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new
                {
                    message = ex.Message
                });
            }
        }
        [HttpPost("{id:long}/test-payout-period")]
        public async Task<IActionResult> TestPayoutPeriod(long id)
        {
            try
            {
                var investment = await _investmentService
                    .PrepareTestPayoutPeriodAsync(id);

                return Ok(investment);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new
                {
                    message = ex.Message
                });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new
                {
                    message = ex.Message
                });
            }
        }
    }
}
