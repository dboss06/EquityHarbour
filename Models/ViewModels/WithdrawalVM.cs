using System.ComponentModel.DataAnnotations;

namespace EquityHarbour.Models.ViewModels
{
    public class WithdrawViewModel
    {
        public decimal CurrentBalance { get; set; }

        [Required(ErrorMessage = "Enter an amount to withdraw.")]
        [Range(1, double.MaxValue, ErrorMessage = "Amount must be greater than 0.")]
        public decimal Amount { get; set; }

        [Required(ErrorMessage = "Bank name is required.")]
        public string BankName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Account name is required.")]
        public string AccountName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Account number is required.")]
        [StringLength(10, MinimumLength = 10, ErrorMessage = "Account number must be 10 digits.")]
        public string AccountNumber { get; set; } = string.Empty;

        public string? Description { get; set; }
    }
}