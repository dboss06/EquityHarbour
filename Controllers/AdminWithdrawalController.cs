using EquityHarbour.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EquityHarbour.Controllers {

    [ApiController]
    [Route("api/admin/withdrawals")]
    [Authorize(Roles = "Admin")]
    public class AdminWithdrawalController : ControllerBase
    {
        private readonly IWithdrawalService _withdrawalService;

        public AdminWithdrawalController(IWithdrawalService withdrawalService)
        {
            _withdrawalService = withdrawalService;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll() {
            var withdrawals = await _withdrawalService.GetAllAsync();
            return Ok(withdrawals);
        }
        [HttpGet("{id:long}")]
        public async Task<IActionResult> GetById(long id) {
            var withdrawal = await _withdrawalService.GetByIdAsync(id);
            if (withdrawal == null)
            {
                return NotFound(new
                {
                    message = "Withdrawal Not Found"
                });
            }
            return Ok(withdrawal);
        }
        [HttpPost("{id:long}/approve")]
        public async Task<IActionResult> Approve(long id)
        {
            try
            {
                var withdrawal = await _withdrawalService.ApproveAsync(id);
                return Ok(withdrawal);
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
        [HttpPost("{id:long}/reject")]
        public async Task<IActionResult> Reject(long id)
        {
            try
            {
                var withdrawal = await _withdrawalService.RejectAsync(id);
                return Ok(withdrawal);
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
