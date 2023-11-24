using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using YIT.__Domain.Entities._Enums;
using YIT.__Domain.Entities.Bases;
using YIT.__Domain.Entities.Models._01Jadual;
using YIT.__Domain.Entities.Models._02Daftar;

namespace YIT.__Domain.Entities.Models._03Akaun
{
    public class AkPO: GenericTransactionFields
    {
        public int Id { get; set; }
        [DisplayName("Tahun Belanjawan")]
        public string? Tahun { get; set; }
        [DisplayName("No Rujukan")]
        public string? NoRujukan { get; set; }
        public DateTime Tarikh { get; set; }
        [DisplayName("Penilaian Perolehan")]
        public int AkPenilaianPerolehanId { get; set; }
        public AkPenilaianPerolehan? AkPenilaianPerolehan { get; set; }
        [DisplayName("Jenis Perolehan")]
        public EnJenisPerolehan EnJenisPerolehan { get; set; }
        [DisplayName("Jumlah RM")]
        [Column(TypeName = "decimal(18, 2)")]
        public decimal Jumlah { get; set; }
        [DisplayName("Kump. Wang")]
        public int JKWId { get; set; }
        public JKW? JKW { get; set; }
        [DisplayName("Pembekal")]
        public int DDaftarAwamId { get; set; }
        public DDaftarAwam? DDaftarAwam{ get; set; }
        public string? NoRujukanLama { get; set; } // dummy
        public ICollection<AkPOObjek>? AkPOObjek { get; set; }
        public ICollection<AkPOPerihal>? AkPOPerihal { get; set; }
        public ICollection<AkPelarasanPO>? AkPelarasanPO { get; set; }
    }
}
