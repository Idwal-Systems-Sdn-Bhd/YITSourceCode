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
    public class AkPenilaianPerolehan : GenericTransactionFields
    {
        public int Id { get; set; }
        [DisplayName("No Rujukan")]
        public string? NoRujukan { get; set; }
        [DisplayName("Tahun Belanjawan")]
        public string? Tahun { get; set; }
        [DisplayName("No Sebutharga")]
        public string? NoSebutHarga { get; set; }
        [DisplayName("Tarikh Mohon")]
        public DateTime Tarikh { get; set; }
        [DisplayName("Tarikh Diperlukan")]
        public DateTime TarikhPerlu { get; set; }
        [DisplayName("Kaedah Perolehan")]
        public EnKaedahPerolehan EnKaedahPerolehan { get; set; }
        [DisplayName("Harga Tawaran RM")]
        [Column(TypeName = "decimal(18, 2)")]
        public decimal HargaTawaran { get; set; }
        [DisplayName("Jumlah RM")]
        [Column(TypeName = "decimal(18, 2)")]
        public decimal Jumlah { get; set; }
        [DisplayName("Justifikasi Pembelian")]
        public string? Sebab { get; set; }
        [DisplayName("Bil Sebutharga")]
        public int? BilSebutharga { get; set; }
        [DisplayName("Mak. Sebutharga")]
        public string? MaklumatSebutHarga { get; set; }
        [DisplayName("Kump. Wang")]
        public int JKWId { get; set; }
        public JKW? JKW { get; set; }
        [DisplayName("Pemohon")]
        public int? DPemohonId { get; set; }
        public string? Jawatan { get; set; }
        public int FlPOInden { get; set; }
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
        public string? Tindakan { get; set; }

        public ICollection<AkPenilaianPerolehanObjek>? AkPenilaianPerolehanObjek { get; set; }
        public ICollection<AkPenilaianPerolehanPerihal>? AkPenilaianPerolehanPerihal { get; set; }

    }
}
