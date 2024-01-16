using System.ComponentModel.DataAnnotations.Schema;
using YIT.__Domain.Entities.Models._01Jadual;

namespace YIT.__Domain.Entities.Models._03Akaun
{
    public class AkJurnalObjek
    {
        public int Id { get; set; }
        public int AkJurnalId { get; set; }
        public AkJurnal? AkJurnal { get; set; }
        public int AkCartaDebitId { get; set; }
        public AkCarta? AkCartaDebit { get; set; }
        public int AkCartaKreditId { get; set; }
        public AkCarta? AkCartaKredit { get; set; }
        public int JKWPTJBahagianDebitId { get; set; }
        public JKWPTJBahagian? JKWPTJBahagianDebit { get; set; }
        public int JKWPTJBahagianKreditId { get; set; }
        public JKWPTJBahagian? JKWPTJBahagianKredit { get; set; }
        [Column(TypeName = "decimal(18, 2)")]
        public decimal Amaun { get; set; }
        public bool IsDebitAbBukuVot { get; set; }
        public bool IsKreditAbBukuVot { get; set; }
    }
}