using System.ComponentModel.DataAnnotations;

namespace EquityHarbour.DTOs.Auth
{
    public class LoginRequest
    {
        [Required]
        [EmailAddress]
        public string EmailAddress { get; set; } = string.Empty;

        [Required]
        public string Password { get; set; } = string.Empty;
    }
}
