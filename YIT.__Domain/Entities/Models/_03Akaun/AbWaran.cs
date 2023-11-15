using YIT.__Domain.Entities._Enums;
using YIT.__Domain.Entities.Bases;
using YIT.__Domain.Entities.Models._02Daftar;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using YIT.__Domain.Entities.Models._01Jadual;

namespace YIT.__Domain.Entities.Models._03Akaun
{
    public class AbWaran : GenericFields
    {
        public int Id { get; set; }
        public string? Tahun { get; set; }
        [DisplayName("No Rujukan")]
        public string? NoRujukan { get; set; }
        public DateTime Tarikh { get; set; }
        [DisplayName("Kumpulan Wang")]
        public int JKWId { get; set; }
        public JKW? JKW { get; set; }
        [DisplayName("Jenis Peruntukan")]
        public EnJenisPeruntukan EnJenisPeruntukan { get; set; }
        [DisplayName("Jenis Pindahan")]
        public int FlJenisPindahan { get; set; } // 0 = dalam bahagian; 1 = antara bahagian
        [Column(TypeName = "decimal(18, 2)")]
        public decimal Jumlah { get; set; }
        public string? Sebab { get; set; }
        public int FlPosting { get; set; }
        public int? DPekerjaPostingId { get; set; }
        public DPekerja? DPekerjaPosting { get; set; }
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
        public string? Tindakan { get; set; }
        public ICollection<AbWaranObjek>? AbWaranObjek { get; set; }

    }
}
