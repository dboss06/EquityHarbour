using EquityHarbour.Areas.Admin.Models;
using EquityHarbour.Models;
using EquityHarbour.Models.Enums;
using EquityHarbour.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace EquityHarbour.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin")]
    public class DashboardController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IDepositService _depositService;
        private readonly IWithdrawalService _withdrawalService;
        private readonly IInvestmentService _investmentService;

        public DashboardController(
            UserManager<ApplicationUser> userManager,
            IDepositService depositService,
            IWithdrawalService withdrawalService,
            IInvestmentService investmentService)
        {
            _userManager = userManager;
            _depositService = depositService;
            _withdrawalService = withdrawalService;
            _investmentService = investmentService;
        }

        public async Task<IActionResult> Index()
        {
            ViewData["Title"] = "Dashboard";
            ViewData["PageTitle"] = "Admin Dashboard";

            var users = await _userManager.GetUsersInRoleAsync("User");
            var deposits = await _depositService.GetAllAsync();
            var withdrawals = await _withdrawalService.GetAllAsync();
            var investments = await _investmentService.GetAllAsync();

            var recentActivity = deposits
                .Select(d => new RecentActivityItem
                {
                    Reference = d.Reference,
                    Type = "Deposit",
                    Amount = d.Amount,
                    Status = d.Status.ToString(),
                    CreatedAt = d.CreatedAt
                })
                .Concat(withdrawals.Select(w => new RecentActivityItem
                {
                    Reference = w.Reference,
                    Type = "Withdrawal",
                    Amount = w.Amount,
                    Status = w.Status.ToString(),
                    CreatedAt = w.CreatedAt
                }))
                .OrderByDescending(a => a.CreatedAt)
                .Take(8)
                .ToList();

            var model = new DashboardViewModel
            {
                TotalUsers = users.Count,
                ActiveInvestments = investments.Count(i => i.Status == InvestmentStatus.Active),
                MaturedInvestments = investments.Count(i => i.Status == InvestmentStatus.Matured),
                CancelledInvestments = investments.Count(i => i.Status == InvestmentStatus.Cancelled),
                PendingDeposits = deposits.Count(d => d.Status == DepositStatus.Pending),
                PendingWithdrawals = withdrawals.Count(w => w.Status == WithdrawalStatus.Pending),
                TotalDeposits = deposits.Where(d => d.Status == DepositStatus.Completed).Sum(d => d.Amount),
                TotalWithdrawals = withdrawals.Where(w => w.Status == WithdrawalStatus.Completed).Sum(w => w.Amount),
                TotalInvested = investments.Sum(i => i.PrincipalAmount),
                RecentUsers = users.OrderByDescending(u => u.CreatedAt).Take(5).Select(u => new RecentUserItem
                {
                    Id = u.Id,
                    FullName = u.FullName,
                    Email = u.Email ?? ""
                }).ToList(),
                RecentActivity = recentActivity
            };

            return View(model);
        }
    }
}