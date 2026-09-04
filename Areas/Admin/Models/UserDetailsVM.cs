using EquityHarbour.DTOs;
using EquityHarbour.DTOs.Investments;
using EquityHarbour.Models;

namespace EquityHarbour.Areas.Admin.Models
{
    public class UserDetailsViewModel
    {
        public string Id { get; set; } = string.Empty;
        public string FullName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string UserName { get; set; } = string.Empty;
        public bool IsActive { get; set; }
        public decimal WalletBalance { get; set; }
        public List<InvestmentDto> Investments { get; set; } = new();
        public List<WalletTransactionDTO> Transactions { get; set; } = new();
        public string? ReferredById { get; set; }
        public string? ReferredByFullName { get; set; }
        public string? ReferredByEmail { get; set; }
        public List<ApplicationUser> DirectReferrals { get; set; } = new();
        public List<ApplicationUser> SecondLevelReferrals { get; set; } = new();
        public List<ApplicationUser> ThirdLevelReferrals { get; set; } = new();
        public HashSet<string> QualifiedReferralIds { get; set; } = new();
    }
}