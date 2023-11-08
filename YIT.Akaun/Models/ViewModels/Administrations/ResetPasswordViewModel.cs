using System.ComponentModel.DataAnnotations;

namespace YIT.Akaun.Models.ViewModels.Administrations
{
    public class ResetPasswordViewModel
    {
        [Required(ErrorMessage = "Emel Diperlukan.")]
        [EmailAddress]
        [Display(Name = "Emel")]
        public string? Email { get; set; }

        [Required(ErrorMessage = "Katalaluan Diperlukan.")]
        [StringLength(100, ErrorMessage = "Minimum {2} aksara diperlukan untuk {0}.", MinimumLength = 6)]
        [DataType(DataType.Password)]
        [Display(Name = "Katalaluan")]
        public string? Password { get; set; }

        [DataType(DataType.Password)]
        [Display(Name = "Pengesahan Katalaluan")]
        [Required(ErrorMessage = "Pengesahan Katalaluan Diperlukan.")]
        [Compare("Password", ErrorMessage = "Katalaluan dan pengesahan katalaluan tidak sama")]
        public string? ConfirmedPassword { get; set; }

        public string? Code { get; set; }
    }
}
