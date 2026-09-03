using EquityHarbour.DTOs.Deposits;
using EquityHarbour.Models;
using EquityHarbour.Models.ViewModels;
using EquityHarbour.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace EquityHarbour.Controllers
{
    [Authorize]
    public class DepositController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IWalletService _walletService;
        private readonly IDepositService _depositService;
        private readonly IDepositAccountService _accountService; private readonly IWebHostEnvironment _environment;

        public DepositController(UserManager<ApplicationUser> userManager, IWalletService walletService, IDepositService depositService, IDepositAccountService accountService, IWebHostEnvironment environment)
        {
            _userManager = userManager;
            _walletService = walletService;
            _depositService = depositService;
            _accountService = accountService;
            _environment = environment;
        }

        public async Task<IActionResult> Index()
        {
            var user = await _userManager.GetUserAsync(User);
            var wallet = await _walletService.GetUserWalletAsync(user.Id);
            var accounts = await _accountService.GetActiveAsync();

            return View(new DepositViewModel
            {
                CurrentBalance = wallet?.AvailableBalance ?? 0,
                Accounts = accounts
            });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(DepositViewModel model, int? accountId)
        {
            var user = await _userManager.GetUserAsync(User);

            if (!ModelState.IsValid)
            {
                var wallet = await _walletService.GetUserWalletAsync(user.Id);
                model.CurrentBalance = wallet?.AvailableBalance ?? 0;
                model.Accounts = await _accountService.GetActiveAsync();
                return View("Index", model);
            }

            try
            {
                var proofImagePath = await SaveProofImageAsync(model.ProofImage);

                var deposit = await _depositService.CreateAsync(user.Id, new CreateDepositRequest
                {
                    Amount = model.Amount,
                    Description = model.Description,
                    DepositAccountId = accountId,
                    ProofImagePath = proofImagePath,
                    UserProvidedReference = model.UserProvidedReference
                });

                return RedirectToAction("Pending", new
                {
                    reference = deposit.Reference,
                    bank = deposit.AccountBankName,
                    number = deposit.AccountNumber,
                    name = deposit.AccountName
                });
            }
            catch (Exception ex)
            {
                ModelState.AddModelError(string.Empty, ex.Message);
                var wallet = await _walletService.GetUserWalletAsync(user.Id);
                model.CurrentBalance = wallet?.AvailableBalance ?? 0;
                model.Accounts = await _accountService.GetActiveAsync();
                return View("Index", model);
            }
        }

        public IActionResult Pending(string reference, string? bank, string? number, string? name)
        {
            ViewData["Reference"] = reference;
            ViewData["Bank"] = bank;
            ViewData["Number"] = number;
            ViewData["Name"] = name;
            return View();
        }
        private async Task<string?> SaveProofImageAsync(IFormFile? file)
        {
            if (file == null) return null;

            var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".webp" };
            var extension = Path.GetExtension(file.FileName).ToLowerInvariant();

            if (!allowedExtensions.Contains(extension))
            {
                throw new ArgumentException("Only JPG, PNG and WEBP images are allowed.");
            }
            if (file.Length > 5 * 1024 * 1024)
            {
                throw new ArgumentException("Maximum file size is 5MB.");
            }

            var uploadsFolder = Path.Combine(_environment.WebRootPath, "uploads", "deposits");
            Directory.CreateDirectory(uploadsFolder);

            var fileName = $"{Guid.NewGuid()}{extension}";
            var filePath = Path.Combine(uploadsFolder, fileName);

            using var stream = new FileStream(filePath, FileMode.Create);
            await file.CopyToAsync(stream);

            return "/uploads/deposits/" + fileName;
        }

    }
}
