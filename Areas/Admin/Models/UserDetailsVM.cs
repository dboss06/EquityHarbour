using EquityHarbour.DTOs;
using EquityHarbour.DTOs.Investments;

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
    }
}