using System.ComponentModel.DataAnnotations;

namespace YIT.Akaun.Models.ViewModels.Forms
{
    public class PenyataFormModel
    {
        public int? id { get; set; }
        [Display(Name = "Kumpulan Wang")]
        public int? JKWId { get; set; }
        [Display(Name = "Bahagian")]
        public int? JBahagianId { get; set; }
        [Required(ErrorMessage = "Tahun Diperlukan")]
        public string? Tahun1 { get; set; }
        public string? Tahun2 { get; set; }
        public string? Tahun3 { get; set; }
        public string? kataKunciDari { get; set; }
        public string? kataKunciHingga { get; set; }
        [Display(Name = "Tarikh Dari")]
        public DateTime? TarDari1 { get; set; }
        [Display(Name = "Tarikh Hingga")]
        public DateTime? TarHingga1 { get; set; }
        [Display(Name = "Bank")]
        public int? AkBankId { get; set; }
        [Display(Name = "Paras")]
        public int? ParasId { get; set; }
        [Display(Name = "Kod Akaun")]
        public int? AkCartaId { get; set; }
    }
}
