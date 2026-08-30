using System.ComponentModel.DataAnnotations;

namespace EquityHarbour.Models.ViewModels
{
    public class BankAccountViewModel
    {
        [Required(ErrorMessage = "Account number is required.")]
        [StringLength(10, MinimumLength = 10, ErrorMessage = "Account number must be 10 digits.")]
        public string AccountNumber { get; set; } = string.Empty;

        [Required(ErrorMessage = "Bank name is required.")]
        public string BankName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Account name is required.")]
        public string AccountName { get; set; } = string.Empty;
    }
}