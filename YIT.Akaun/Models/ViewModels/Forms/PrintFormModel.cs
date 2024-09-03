using System.ComponentModel;
using YIT.__Domain.Entities._Enums;

namespace YIT.Akaun.Models.ViewModels.Forms
{
    public class PrintFormModel
    {
        public int? JKWId { get; set; }
        public int? JPTJId { get; set; }
        public int? JBahagianId { get; set; }
        public int? AkCartaId { get; set; }
        public string? kodLaporan { get; set; }
        public string? tahun1 { get; set; }
        public string? bulan1 { get; set; }
        public string? tarikhDari { get; set; }
        public string? tarikhHingga { get; set; }
        public DateTime? tarDari1 { get; set; }
        public DateTime? tarHingga1 { get; set; }
        public string? susunan { get; set; }
        public int? akBankId { get; set; }
        public int? tunai { get; set; }
        [DisplayName("Jenis Perolehan")]
        public EnJenisPerolehan enJenisPerolehan { get; set; }
        [DisplayName("Kumpulan Wang")]
        public int? jKWId { get; set; }
        [DisplayName("Bahagian")]
        public int? jBahagianId { get; set; }
        public string? tahun { get; set; }
        public EnJenisPeruntukan enJenisPeruntukan { get; set; }
        public EnStatusBorang enStatusBorang { get; set; }
        public string? searchString1 { get; set; }
        public string? searchString2 { get; set; }
        public int? dDaftarAwamId { get; set; }
    }
}