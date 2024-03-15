using System.ComponentModel.DataAnnotations.Schema;
using YIT.__Domain.Entities.Models._02Daftar;

namespace YIT.__Domain.Entities.Models._03Akaun
{
    public class AkRekup
    {
        public int Id { get; set; }
        public string? NoRujukan { get; set; }
        [Column(TypeName = "decimal(18, 2)")]
        public decimal Jumlah { get; set; }
        public int DPanjarId { get; set; }
        public DPanjar? DPanjar { get; set; }
        public bool IsLinked { get; set; }
    }
}