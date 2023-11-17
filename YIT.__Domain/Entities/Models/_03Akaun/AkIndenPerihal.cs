using System.ComponentModel.DataAnnotations.Schema;

namespace YIT.__Domain.Entities.Models._03Akaun
{
    public class AkIndenPerihal
    {
        public int Id { get; set; }
        public int AkIndenId { get; set; }
        public AkInden? AkInden { get; set; }
        [Column(TypeName = "decimal(18, 2)")]
        public decimal Bil { get; set; }
        public string? Perihal { get; set; }
        [Column(TypeName = "decimal(18, 2)")]
        public decimal Kuantiti { get; set; }
        public string? Unit { get; set; }
        [Column(TypeName = "decimal(18, 2)")]
        public decimal Harga { get; set; }
        [Column(TypeName = "decimal(18, 2)")]
        public decimal Amaun { get; set; }
    }
}