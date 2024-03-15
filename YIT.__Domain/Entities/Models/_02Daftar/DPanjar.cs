using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using YIT.__Domain.Entities.Bases;
using YIT.__Domain.Entities.Models._01Jadual;
using YIT.__Domain.Entities.Models._03Akaun;

namespace YIT.__Domain.Entities.Models._02Daftar
{
    public class DPanjar : GenericFields
    {
        public int Id { get; set; }
        public string? Kod { get; set; }
        [Display(Name = "Bahagian")]
        public int? JKWPTJBahagianId { get; set; }
        public JKWPTJBahagian? JKWPTJBahagian { get; set; }
        [Display(Name = "Cawangan")]
        public int JCawanganId { get; set; }
        public JCawangan? JCawangan { get; set; }
        public string? Catatan { get; set; }
        [Display(Name = "Kod Panjar")]
        public int AkCartaId { get; set; }
        public AkCarta? AkCarta { get; set; }
        [Column(TypeName = "decimal(18, 2)")]
        [Display(Name = "Had Jumlah RM")]
        public decimal HadJumlah { get; set; }
        public ICollection<DPanjarPemegang>? DPanjarPemegang { get; set; }
        public ICollection<AkRekup>? AkRekup { get; set; }
        public ICollection<AkPanjarLejar>? AkPanjarLejar { get; set; }
    }
}