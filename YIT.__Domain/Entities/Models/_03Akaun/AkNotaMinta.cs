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
using YIT.__Domain.Entities.Models._50LHDN;

namespace YIT.__Domain.Entities.Models._03Akaun
{
    public class AkNotaMinta : GenericTransactionFields
    {
        public int Id { get; set; }
        [DisplayName("No Rujukan")]
        public string? NoRujukan { get; set; }
        [DisplayName("Tahun Belanjawan")]
        public string? Tahun { get; set; }
        [DisplayName("Tarikh Mohon")]
        public DateTime Tarikh { get; set; }
        [DisplayName("Tarikh Diperlukan")]
        public DateTime TarikhPerlu { get; set; }
        [DisplayName("Kaedah Perolehan")]
        public EnKaedahPerolehan EnKaedahPerolehan { get; set; }
        [DisplayName("Jumlah RM")]
        [Column(TypeName = "decimal(18, 2)")]
        public decimal Jumlah { get; set; }
        public string? Sebab { get; set; }
        [DisplayName("Kump. Wang")]
        public int JKWId { get; set; }
        public JKW? JKW { get; set; }
        [DisplayName("Pemohon")]
        public int? DPemohonId { get; set; }
        public string? Jawatan { get; set; }
        public DPekerja? DPemohon { get; set; }
        [DisplayName("Cadangan Pembekal")]
        public int DDaftarAwamId { get; set; }
        public DDaftarAwam? DDaftarAwam { get; set; }
        [DisplayName("MSIC")]
        public int? LHDNMSICId { get; set; }
        public LHDNMSIC? LHDNMSIC { get; set; }
        [Column(TypeName = "decimal(18, 2)")]
        public decimal JumlahCukai { get; set; }
        [Column(TypeName = "decimal(18, 2)")]
        public decimal JumlahTanpaCukai { get; set; }
        public ICollection<AkNotaMintaObjek>? AkNotaMintaObjek { get; set; }
        public ICollection<AkNotaMintaPerihal>? AkNotaMintaPerihal { get; set; }
        [NotMapped]
        public AkPV? AkPV { get; set; }

    }
}
