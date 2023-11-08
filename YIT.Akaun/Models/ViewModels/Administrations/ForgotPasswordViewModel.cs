using System.ComponentModel.DataAnnotations;

namespace YIT.Akaun.Models.ViewModels.Administrations
{
    public class ForgotPasswordViewModel
    {
        [Required(ErrorMessage = "Emel diperlukan")]
        [EmailAddress]
        public string Emel { get; set; } = string.Empty;
    }
}
