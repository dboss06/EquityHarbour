using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using EquityHarbour.Services;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;

namespace EquityHarbour.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class WalletController : ControllerBase
    {
        private readonly IWalletService _walletService;

        public WalletController(IWalletService walletService) { 
            _walletService = walletService;
        }

        [HttpGet]
        public async Task<IActionResult> GetWallet() {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized();
            }
            var wallet = await _walletService.GetUserWalletAsync(userId);
            if (wallet == null) {
                return NotFound(new
                {
                    message = "Wallet Not Found"
                });
            }
            return Ok(new
            {
                wallet.Id,
                wallet.AvailableBalance,
                wallet.InvestedBalance,
                wallet.TotalDeposited,
                wallet.TotalWithdrawn,
                wallet.TotalProfit,
            });
        }
        [HttpGet("transactions")]
        public async Task<IActionResult> GetTransactions()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized();
            }

            var transactions = await _walletService.GetTransactionsAsync(userId);

            return Ok(transactions);
        }
    }
}
