using EquityHarbour.Models;
using EquityHarbour.Models.ViewModels;
using EquityHarbour.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace EquityHarbour.Controllers
{
    [Authorize]
    public class GiftController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IGiftCodeService _giftCodeService;

        public GiftController(UserManager<ApplicationUser> userManager, IGiftCodeService giftCodeService)
        {
            _userManager = userManager;
            _giftCodeService = giftCodeService;
        }

        public IActionResult Index()
        {
            return View(new GiftCodeViewModel());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Redeem(GiftCodeViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View("Index", model);
            }

            var user = await _userManager.GetUserAsync(User);

            try
            {
                var amount = await _giftCodeService.RedeemAsync(user.Id, model.Code);
                TempData["Success"] = $"₦{amount:N2} credited to your wallet!";
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                ModelState.AddModelError(string.Empty, ex.Message);
                return View("Index", model);
            }
        }
    }
}