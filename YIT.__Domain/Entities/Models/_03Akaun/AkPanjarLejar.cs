using System.ComponentModel.DataAnnotations.Schema;

namespace YIT.__Domain.Entities.Models._03Akaun
{
    public class AkPanjarLejar
    {
        public int Id { get; set; }
        public int AkPanjarId { get; set; }
        public AkPanjar? AkPanjar { get; set; }
        public int? AkCVId { get; set; }
        public AkCV? AkCV { get; set; }
        public int? AkPVId { get; set; }
        public AkPV? AkPV { get; set; }
        public DateTime Tarikh { get; set; }
        [Column(TypeName = "decimal(18, 2)")]
        public decimal Debit { get; set; }
        [Column(TypeName = "decimal(18, 2)")]
        public decimal Kredit { get; set; }
        public int? AkRekupId { get; set; }
        public AkRekup? AkRekup { get; set; }
        
    }
}
