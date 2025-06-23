using System.ComponentModel.DataAnnotations;

namespace GarageMasterBE.Models
{
    public class ConfirmEmailRequest
    {
        [Required]
        [EmailAddress]
        public string Email { get; set; } = null!;

        [Required]
        [StringLength(6, MinimumLength = 6, ErrorMessage = "Mã xác thực gồm 6 chữ số.")]
        public string Code { get; set; } = null!;
    }
}
