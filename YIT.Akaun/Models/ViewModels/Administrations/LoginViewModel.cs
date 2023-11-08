using System.ComponentModel.DataAnnotations;
using System.Xml.Linq;

namespace YIT.Akaun.Models.ViewModels.Administrations
{
    public class LoginViewModel
    {
        [Required(ErrorMessage = "Emel diperlukan")]
        [EmailAddress]
        public string Emel { get; set; } = string.Empty;

        [Required(ErrorMessage = "Katalaluan diperlukan")]
        [DataType(DataType.Password)]
        public string Katalaluan { get; set; } = string.Empty;

        [Display(Name = "Ingat saya?")]
        public bool IngatSaya { get; set; }
    }
}
