using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using YIT.__Domain.Entities.Bases;
using YIT.__Domain.Entities.Models._01Jadual;
using YIT.__Domain.Entities.Models._50LHDN;

namespace YIT.__Domain.Entities.Models._03Akaun
{
    public class AkPelarasanInden : GenericTransactionFields
    {
        public int Id { get; set; }
        public string? Tahun { get; set; }
        [DisplayName("No Rujukan")]
        public string? NoRujukan { get; set; }
        public DateTime Tarikh { get; set; }
        [DisplayName("No Inden Kerja")]
        public int AkIndenId { get; set; }
        public AkInden? AkInden { get; set; }
        [DisplayName("Jumlah RM")]
        [Column(TypeName = "decimal(18, 2)")]
        public decimal Jumlah { get; set; }
        [DisplayName("Kump. Wang")]
        public int JKWId { get; set; }
        public JKW? JKW { get; set; }
        public string? Ringkasan { get; set; }
        public int? LHDNMSICId { get; set; }
        public LHDNMSIC? LHDNMSIC { get; set; }
        [Column(TypeName = "decimal(18, 2)")]
        public decimal JumlahCukai { get; set; }
        [Column(TypeName = "decimal(18, 2)")]
        public decimal JumlahTanpaCukai { get; set; }
        public string? NoRujukanLama { get; set; } // dummy
        public ICollection<AkPelarasanIndenObjek>? AkPelarasanIndenObjek { get; set; }
        public ICollection<AkPelarasanIndenPerihal>? AkPelarasanIndenPerihal { get; set; }
    }
}
