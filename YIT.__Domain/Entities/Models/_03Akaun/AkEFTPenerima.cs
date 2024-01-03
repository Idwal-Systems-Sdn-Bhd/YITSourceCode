using System.ComponentModel.DataAnnotations.Schema;
using YIT.__Domain.Entities._Enums;
using YIT.__Domain.Entities.Models._01Jadual;

namespace YIT.__Domain.Entities.Models._03Akaun
{
    public class AkEFTPenerima
    {
        public int Id { get; set; }
        public int AkEFTId { get; set; }
        public AkEFT? AkEFT { get; set; }
        public int AkPVId { get; set; }
        public AkPV? AkPV { get; set; }
        public EnStatusProses EnStatusEFT { get; set; }
        public string? SebabGagal { get; set; }
        public DateTime? TarikhKredit { get; set; }
        public int? Bil { get; set; }
        public string? NoPendaftaranPenerima { get; set; }
        public string? NamaPenerima { get; set; }
        public string? Catatan { get; set; }
        public int JCaraBayarId { get; set; }
        public JCaraBayar? JCaraBayar { get; set; }
        public int? JBankId { get; set; }
        public JBank? JBank { get; set; }
        public string? NoAkaunBank { get; set; }
        public string? KodM2E { get; set; }
        public string? NoCek { get; set; }
        [Column(TypeName = "decimal(18, 2)")]
        public decimal Amaun { get; set; }
        public EnJenisId EnJenisId { get; set; }
    }
}