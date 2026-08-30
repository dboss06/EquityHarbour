using EquityHarbour.Models;
using EquityHarbour.Models.ViewModels;
using EquityHarbour.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace EquityHarbour.Controllers
{
    [Authorize]
    public class ProfileController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly IWalletService _walletService;

        public ProfileController(
            UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager,
            IWalletService walletService)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _walletService = walletService;
        }

        public async Task<IActionResult> Index()
        {
            var user = await _userManager.GetUserAsync(User);
            var wallet = await _walletService.GetUserWalletAsync(user.Id);

            return View(new ProfileViewModel
            {
                Balance = wallet?.AvailableBalance ?? 0
            });
        }

        [HttpGet]
        public IActionResult ResetPassword() => View(new ChangePasswordViewModel());

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ResetPassword(ChangePasswordViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var user = await _userManager.GetUserAsync(User);
            var result = await _userManager.ChangePasswordAsync(user, model.CurrentPassword, model.NewPassword);

            if (!result.Succeeded)
            {
                foreach (var error in result.Errors)
                    ModelState.AddModelError(string.Empty, error.Description);
                return View(model);
            }

            // Keeps the current session valid after the password/security stamp changes
            await _signInManager.RefreshSignInAsync(user);

            TempData["Success"] = "Password updated successfully.";
            return RedirectToAction("Index");
        }
    }
}