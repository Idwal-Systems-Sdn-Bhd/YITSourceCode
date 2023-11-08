using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel;
using Microsoft.AspNetCore.Mvc.Rendering;
using YIT.__Domain.Entities.Models._02Daftar;

namespace YIT.__Domain.Entities.Administrations
{
    public class ApplicationUser : IdentityUser
    {
        public string? Nama { get; set; }
        [DisplayName("Tandatangan")]
        public string? Tandatangan { get; set; }
        [NotMapped]
        public string? RoleId { get; set; }
        [NotMapped]
        public string? Role { get; set; }
        [NotMapped]
        public List<string>? UserRoles { get; set; }
        [NotMapped]
        public List<IdentityUserRole<string>>? SelectedRoleList { get; set; }
        [NotMapped]
        public IEnumerable<SelectListItem>? RoleList { get; set; }

        //relationship
        public int? DPekerjaId { get; set; }
        public DPekerja? DPekerja { get; set; }
        [DisplayName("Bahagian")]
        public string? JBahagianList { get; set; }
        [NotMapped]
        public List<int>? SelectedJBahagianList { get; set; }
    }
}
