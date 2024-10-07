using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel;
using YIT.__Domain.Entities.Models._01Jadual;

namespace YIT.__Domain.Entities.Models._03Akaun
{
    public class AkAkaunPenyataBank
    {
        public int Id { get; set; }
        public int? AkAkaunId { get; set; }
        public AkAkaun? AkAkaun { get; set; }
        public int AkPenyesuaianBankPenyataBankId { get; set; }
        public AkPenyesuaianBankPenyataBank? AkPenyesuaianBankPenyataBank { get; set; }
        public int? AkPVPenerimaId { get; set; }
        public AkPVPenerima? AkPVPenerima { get; set; }
        [Column(TypeName = "decimal(18, 2)")]
        public decimal Bil { get; set; }
        public bool IsAutoMatch { get; set; }
        public int? JCaraBayarId { get; set; }
        public JCaraBayar? JCaraBayar { get; set; }
        [Column(TypeName = "decimal(18, 2)")]
        public decimal Debit { get; set; }
        [Column(TypeName = "decimal(18, 2)")]
        public decimal Kredit { get; set; }
        public DateTime Tarikh { get; set; }
    }
}