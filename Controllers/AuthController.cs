using EquityHarbour.DTOs.Auth;
using EquityHarbour.Services;
using Microsoft.AspNetCore.Mvc;

namespace EquityHarbour.Controllers
{
    public class AuthController : Controller
    {
        private readonly AuthService _authService;

        public AuthController(AuthService authService)
        {
            _authService = authService;
        }

        [HttpGet]
        public IActionResult Login() => View();

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginRequest request)
        {
            if (!ModelState.IsValid) return View(request);

            var result = await _authService.LoginAsync(request);
            if (result == null)
            {
                ModelState.AddModelError(string.Empty, "Invalid email or password.");
                return View(request);
            }

            return result.Value.Role == "Admin"
                ? RedirectToAction("Index", "Dashboard", new { area = "Admin" })
                : RedirectToAction("Index", "Home");
        }

        [HttpGet]
        public IActionResult Register([FromQuery(Name = "ref")] string? referralCode)
        {
            return View(new RegisterRequest { ReferralCode = referralCode });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(RegisterRequest request)
        {
            if (!ModelState.IsValid) return View(request);

            var result = await _authService.RegisterAsync(request);
            if (!result.Succeeded)
            {
                foreach (var error in result.Errors)
                    ModelState.AddModelError(string.Empty, error.Description);
                return View(request);
            }

            return RedirectToAction("Login");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Logout()
        {
            await _authService.LogoutAsync();
            return RedirectToAction("Login");
        }
    }
}