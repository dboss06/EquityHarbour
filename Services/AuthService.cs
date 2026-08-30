using EquityHarbour.DTOs.Auth;
using EquityHarbour.Models;
using Microsoft.AspNetCore.Identity;
using EquityHarbour.Data;

namespace EquityHarbour.Services
{
    public class AuthService
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly ApplicationDbContext _context;
        private readonly ILogger<AuthService> _logger;
        private readonly IReferralService _referralService;

        public AuthService(
            UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager,
            ILogger<AuthService> logger,
            ApplicationDbContext context,
            IReferralService referralService)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _context = context;
            _logger = logger;
            _referralService = referralService;
        }

        public async Task<IdentityResult> RegisterAsync(RegisterRequest request)
        {
            var user = new ApplicationUser
            {
                FullName = request.FullName,
                Email = request.EmailAddress,
                UserName = request.EmailAddress,
                CreatedAt = DateTime.UtcNow,
                ReferralCode = await _referralService.GenerateUniqueReferralCodeAsync()
            };
            var result = await _userManager.CreateAsync(user, request.Password);
            if (!result.Succeeded)
            {
                _logger.LogInformation("User registration failed for {Email}", user.Email);
                return result;
            }
            var roleResult = await _userManager.AddToRoleAsync(user, "User");
            if (!roleResult.Succeeded)
            {
                _logger.LogError("Failed to assign User role to {UserId}", user.Id);
                await _userManager.DeleteAsync(user);
                return roleResult;
            }

            await _referralService.LinkReferrerAsync(user.Id, request.ReferralCode);

            var wallet = new Wallet
            {
                UserId = user.Id,
                AvailableBalance = 0m,
                InvestedBalance = 0m,
                TotalDeposited = 0m,
                TotalWithdrawn = 0m,
                TotalProfit = 0m
            };
            _context.Wallets.Add(wallet);
            await _context.SaveChangesAsync();
            _logger.LogInformation("New user registered. UserId: {UserId}", user.Id);
            return result;
        }

        // Returns the signed-in user (with role) on success, or null on failure.
        // SignInManager.PasswordSignInAsync issues the auth cookie for us.
        public async Task<(ApplicationUser User, string? Role)?> LoginAsync(LoginRequest request)
        {
            var user = await _userManager.FindByEmailAsync(request.EmailAddress);
            if (user == null)
            {
                _logger.LogInformation("Login failed for {Email}", request.EmailAddress);
                return null;
            }

            var result = await _signInManager.PasswordSignInAsync(
                user, request.Password, isPersistent: true, lockoutOnFailure: false);

            if (!result.Succeeded)
            {
                _logger.LogInformation("Login failed for {Email}", request.EmailAddress);
                return null;
            }

            var roles = await _userManager.GetRolesAsync(user);
            _logger.LogInformation("User logged in. UserId: {UserId}", user.Id);
            return (user, roles.FirstOrDefault());
        }

        public async Task LogoutAsync()
        {
            await _signInManager.SignOutAsync();
        }
    }
}