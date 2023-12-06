using System.ComponentModel.DataAnnotations.Schema;

namespace YIT.__Domain.Entities.Models._03Akaun
{
    public class AkRekup
    {
        public int Id { get; set; }
        public string? NoRujukan { get; set; }
        [Column(TypeName = "decimal(18, 2)")]
        public decimal Jumlah { get; set; }
        public int AkPanjarId { get; set; }
        public AkPanjar? AkPanjar { get; set; }
    }
}