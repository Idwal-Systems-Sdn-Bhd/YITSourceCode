using System.ComponentModel.DataAnnotations.Schema;
using YIT.__Domain.Entities._Enums;
using YIT.__Domain.Entities.Models._50LHDN;

namespace YIT.__Domain.Entities.Models._03Akaun
{
    public class AkPelarasanPOPerihal
    {
        public int Id { get; set; }
        public int AkPelarasanPOId { get; set; }
        public AkPelarasanPO? AkPelarasanPO { get; set; }
        [Column(TypeName = "decimal(18, 2)")]
        public decimal Bil { get; set; }
        public string? Perihal { get; set; }
        [Column(TypeName = "decimal(18, 2)")]
        public decimal Kuantiti { get; set; }
        public int? LHDNUnitUkuranId { get; set; }
        public LHDNUnitUkuran? LHDNUnitUkuran { get; set; }
        public string? Unit { get; set; }
        public EnLHDNJenisCukai EnLHDNJenisCukai { get; set; }
        [Column(TypeName = "decimal(18, 2)")]
        public decimal KadarCukai { get; set; }
        [Column(TypeName = "decimal(18, 2)")]
        public decimal AmaunCukai { get; set; }
        [Column(TypeName = "decimal(18, 2)")]
        public decimal Harga { get; set; }
        [Column(TypeName = "decimal(18, 2)")]
        public decimal Amaun { get; set; }
    }
}