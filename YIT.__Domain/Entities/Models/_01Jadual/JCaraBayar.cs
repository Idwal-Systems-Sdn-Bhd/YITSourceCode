using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using YIT.__Domain.Entities.Bases;
using YIT.__Domain.Entities.Models._03Akaun;

namespace YIT.__Domain.Entities.Models._01Jadual
{
    public class JCaraBayar : GenericFields
    {
        public int Id { get; set; }
        public string? Kod { get; set; }
        public string? Perihal { get; set; }
        [Display(Name = "Cara bayar ini mempunyai had?")]
        public bool IsLimit { get; set; }
        [Display(Name = "Maksimum Amaun RM")]
        [Column(TypeName = "decimal(18, 2)")]
        public decimal MaksAmaun { get; set; }
        public ICollection<AkTerimaCaraBayar>? AkTerimaCaraBayar { get; set; }
        public ICollection<AkPVPenerima>? AkPVPenerima { get; set; }
        public ICollection<AkJanaanProfilPenerima>? AkJanaanProfilPenerima { get; set; }
        public ICollection<AkEFTPenerima>? AkEFTPenerima { get; set; }

    }
}
