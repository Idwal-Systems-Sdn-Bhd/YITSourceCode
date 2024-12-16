using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using YIT.__Domain.Entities._Enums;
using YIT.__Domain.Entities.Administrations;

namespace YIT.Akaun.Models.ViewModels.Forms
{
    public class PrintFormModel
    {
        public string? kodLaporan { get; set; }
        public string? tahun1 { get; set; }
        public string? bulan1 { get; set; }
        public string? tarikhDari { get; set; }
        public string? tarikhHingga { get; set; }
        public string? susunan { get; set; }
        public string? Username { get; set; }
        public string? Tajuk1 { get; set; }
        public string? Tajuk2 { get; set; }
        public string? FormatLaporan { get; set; }
        public CompanyDetails? CompanyDetails { get; set; }
        [Display(Name = "PTJ")]
        public int? JPTJId { get; set; } 
        [Display(Name = "JKW/PTJ/Bahagian")]
        public int? JKWPTJBahagianId { get; set; }
        [Display(Name = "Tarikh Dari")]
        public DateTime? tarDari1 { get; set; }
        [Display(Name = "Tarikh Hingga")]
        public DateTime? tarHingga1 { get; set; }
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
        [Display(Name = "Kod Akaun")]
        public int? AkCartaId { get; set; }
        public string? Tahun1 { get; set; }
        [RegularExpression(@"^\d{2}$")]
        public string? Bulan { get; set; }
        [Display(Name = "Bank")]
        public int? AkBankId { get; set; }
        [Display(Name = "Paras")]
        public EnParas EnParas { get; set; }
        public int? dPekerjaId1 { get; set; }
        public int? dPekerjaId2 { get; set; }
        public int? dPekerjaId3 { get; set; }
        [DisplayName("Cawangan")]
        public int? jCawanganId { get; set; }
    }
}