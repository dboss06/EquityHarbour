using EquityHarbour.Models;
using EquityHarbour.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace EquityHarbour.Controllers
{
    [Authorize]
    public class ReferralsController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IReferralService _referralService;

        public ReferralsController(UserManager<ApplicationUser> userManager, IReferralService referralService)
        {
            _userManager = userManager;
            _referralService = referralService;
        }

        public async Task<IActionResult> Invite()
        {
            var user = await _userManager.GetUserAsync(User);
            var inviteLink = $"{Request.Scheme}://{Request.Host}/Auth/Register?ref={user.ReferralCode}";

            ViewData["InviteLink"] = inviteLink;
            return View();
        }

        public async Task<IActionResult> Index(string level = "direct")
        {
            var user = await _userManager.GetUserAsync(User);

            var members = level switch
            {
                "second" => await _referralService.GetSecondLevelReferralsAsync(user.Id),
                "third" => await _referralService.GetThirdLevelReferralsAsync(user.Id),
                _ => await _referralService.GetDirectReferralsAsync(user.Id)
            };

            var qualifiedIds = await _referralService.GetQualifiedUserIdsAsync(members.Select(m => m.Id));

            ViewData["ActiveLevel"] = level;
            ViewData["QualifiedIds"] = qualifiedIds;
            return View(members);
        }
    }
}