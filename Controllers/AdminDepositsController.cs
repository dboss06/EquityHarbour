using EquityHarbour.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EquityHarbour.Controllers
{
    [Route("api/admin/deposits")]
    [ApiController]
    [Authorize(Roles ="Admin")]
    public class AdminDepositsController : ControllerBase
    {
        private readonly IDepositService _depositService;
        public AdminDepositsController(IDepositService depositService)
        {
            _depositService = depositService;
        }
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var deposits = await _depositService.GetAllAsync();
            return Ok(deposits);
        }
        [HttpGet("{id:long}")]
        public async Task<IActionResult> GetById(long id)
        {
            var deposit = await _depositService.GetByIdAsync(id);
            if (deposit == null)
            {
                return NotFound(new
                {
                    message = "Deposit not found."
                });
            }
            return Ok(deposit);
        }
        [HttpPost("{id:long}/approve")]
        public async Task<IActionResult> Approve(long id)
        {
            try
            {
                var deposit = await _depositService.ApproveAsync(id);
                return Ok(deposit);
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
                var deposit = await _depositService.RejectAsync(id);
                return Ok(deposit);
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
