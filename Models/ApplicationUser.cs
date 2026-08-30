using Microsoft.AspNetCore.Identity;

namespace EquityHarbour.Models
{
    public class ApplicationUser : IdentityUser
    {
        public string FullName { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public bool IsActive { get; set; } = true;
        public DateTime? DeactivatedAt { get; set; }
        public Wallet? Wallet { get; set; }
        public ICollection<Investment> Investments { get; set; } = new List<Investment>();
        public string ReferralCode { get; set; } = string.Empty;
        public string? ReferredByUserId { get; set; }
        public ApplicationUser? ReferredBy { get; set; }
    }
}