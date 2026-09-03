using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;

namespace EquityHarbour.Models.ViewModels
{
    public class DepositViewModel
    {
        public decimal CurrentBalance { get; set; }

        [Required(ErrorMessage = "Enter an amount to deposit.")]
        [Range(3000, double.MaxValue, ErrorMessage = "Minimum deposit is ₦3,000.")]
        public decimal Amount { get; set; }

        public string? Description { get; set; }
        public List<EquityHarbour.DTOs.DepositAccounts.DepositAccountDto> Accounts { get; set; } = new();
        public IFormFile? ProofImage { get; set; }
        public string? UserProvidedReference { get; set; }
    }
}