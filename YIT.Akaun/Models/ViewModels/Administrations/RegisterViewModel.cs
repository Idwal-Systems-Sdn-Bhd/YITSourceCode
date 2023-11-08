using System.ComponentModel.DataAnnotations;
using System.ComponentModel;
using Microsoft.AspNetCore.Mvc.Rendering;
using YIT.__Domain.Entities.Models._02Daftar;

namespace YIT.Akaun.Models.ViewModels.Administrations
{
    public class RegisterViewModel
    {
        [Required(ErrorMessage = "Emel Diperlukan.")]
        [EmailAddress]
        [Display(Name = "Emel")]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "Katalaluan Diperlukan.")]
        [StringLength(100, ErrorMessage = "{0} must be at least {2} characters long.", MinimumLength = 6)]
        [DataType(DataType.Password)]
        [Display(Name = "Katalaluan")]
        public string Password { get; set; } = string.Empty;

        [DataType(DataType.Password)]
        [Display(Name = "Pengesahan Katalaluan")]
        [Compare("Password", ErrorMessage = "Katalaluan dan pengesahan katalaluan tidak sama")]
        public string ConfirmPassword { get; set; } = string.Empty;

        [Display(Name = "Nama Penuh")]
        public string Nama { get; set; } = string.Empty;

        public IEnumerable<SelectListItem>? RoleList { get; set; }
        [Display(Name = "Peranan")]
        [Required(ErrorMessage = "Peranan Diperlukan.")]
        public string RoleSelected { get; set; } = string.Empty;
        [DisplayName("Anggota")]
        [Required(ErrorMessage = "Anggota Diperlukan.")]
        public int? DPekerjaId { get; set; }
        public DPekerja? DPekerja { get; set; }
        [DisplayName("Bahagian")]
        public string JBahagianList { get; set; } = string.Empty;
        [Required(ErrorMessage = "Bahagian Diperlukan.")]
        public List<int>? SelectedJBahagianList { get; set; }
    }
}
