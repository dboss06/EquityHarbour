using System.ComponentModel.DataAnnotations;

namespace EquityHarbour.Models.ViewModels
{
    public class GiftCodeViewModel
    {
        [Required(ErrorMessage = "Enter a gift code.")]
        public string Code { get; set; } = string.Empty;
    }
}