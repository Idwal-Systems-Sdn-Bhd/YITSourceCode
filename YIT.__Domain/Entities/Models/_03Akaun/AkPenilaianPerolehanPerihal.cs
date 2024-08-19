using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using YIT.__Domain.Entities._Enums;
using YIT.__Domain.Entities.Models._50LHDN;

namespace YIT.__Domain.Entities.Models._03Akaun
{
    public class AkPenilaianPerolehanPerihal
    {
        public int Id { get; set; }
        public int AkPenilaianPerolehanId { get; set; }
        public AkPenilaianPerolehan? AkPenilaianPerolehan { get; set; }
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
