using EquityHarbour.DTOs.GiftCodes;
using EquityHarbour.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EquityHarbour.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin")]
    public class GiftCodesController : Controller
    {
        private readonly IGiftCodeService _giftCodeService;

        public GiftCodesController(IGiftCodeService giftCodeService)
        {
            _giftCodeService = giftCodeService;
        }

        public async Task<IActionResult> Index()
        {
            ViewData["Title"] = "Gift Codes";
            ViewData["PageTitle"] = "Gift Codes";
            var codes = await _giftCodeService.GetAllAsync();
            return View(codes);
        }

        public IActionResult Create()
        {
            ViewData["Title"] = "Create Gift Code";
            ViewData["PageTitle"] = "Create Gift Code";
            return View(new CreateGiftCodeRequest());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CreateGiftCodeRequest request)
        {
            try
            {
                var code = await _giftCodeService.CreateAsync(request);
                TempData["Success"] = $"Gift code {code.Code} created.";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                ModelState.AddModelError(string.Empty, ex.Message);
                ViewData["Title"] = "Create Gift Code";
                ViewData["PageTitle"] = "Create Gift Code";
                return View(request);
            }
        }
    }
}