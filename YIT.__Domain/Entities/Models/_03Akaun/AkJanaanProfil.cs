using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using YIT.__Domain.Entities._Enums;
using YIT.__Domain.Entities.Bases;
using YIT.__Domain.Entities.Models._01Jadual;
using YIT.__Domain.Entities.Models._02Daftar;

namespace YIT.__Domain.Entities.Models._03Akaun
{
    public class AkJanaanProfil : GenericFields
    {
        public int Id { get; set; }
        public string? NoRujukan { get; set; }
        [Display(Name = "Cawangan")]
        public int JCawanganId { get; set; }
        public JCawangan? JCawangan { get; set; }
        [Display(Name = "Jumlah RM")]
        [Column(TypeName = "decimal(18, 2)")]
        public decimal Jumlah { get; set; }
        public DateTime Tarikh { get; set; }
        [Display(Name = "Jenis Modul")]
        public EnJenisModulProfil EnJenisModulProfil { get; set; }
        public ICollection<AkJanaanProfilPenerima>? AkJanaanProfilPenerima{ get; set; }
        public ICollection<AkPV>? AkPV { get; set; }

    }
}