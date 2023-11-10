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
    public class AkNotaMinta : GenericFields
    {
        public int Id { get; set; }
        [DisplayName("No Rujukan")]
        public string? NoRujukan { get; set; }
        public string? Tahun { get; set; }
        public DateTime Tarikh { get; set; }
        [DisplayName("Kaedah Perolehan")]
        public EnKaedahPerolehan EnKaedahPerolehan { get; set; }
        [Column(TypeName = "decimal(18, 2)")]
        public decimal Jumlah { get; set; }
        public string? Sebab { get; set; }
        [DisplayName("Kump. Wang")]
        public int JKWId { get; set; }
        public JKW? JKW { get; set; }
        [DisplayName("Pemohon")]
        public int DPemohonId { get; set; }
        public string? Jawatan { get; set; }
        public DPekerja? DPemohon { get; set; }
        [DisplayName("Cadangan Pembekal")]
        public int DDaftarAwamId { get; set; }
        public DDaftarAwam? DDaftarAwam { get; set; }
        public int? DPekerjaPostingId { get; set; }
        public DPekerja? DPekerjaPosting { get; set; }
        public int FlPosting { get; set; }
        public DateTime? TarikhPosting { get; set; }
        public int? DPengesahId { get; set; }
        public DKonfigKelulusan? DPengesah { get; set; }
        public DateTime? TarikhSah { get; set; }
        public int? DPenyemakId { get; set; }
        public DKonfigKelulusan? DPenyemak { get; set; }
        public DateTime? TarikhSemak { get; set; }
        public int? DPelulusId { get; set; }
        public DKonfigKelulusan? DPelulus { get; set; }
        public DateTime? TarikhLulus { get; set; }
        [DisplayName("Status")]
        public EnStatusBorang EnStatusBorang { get; set; }
        public ICollection<AkNotaMintaObjek>? AkNotaMintaObjek { get; set; }
        public ICollection<AkNotaMintaPerihal>? AkNotaMintaPerihal { get; set; }

    }
}
