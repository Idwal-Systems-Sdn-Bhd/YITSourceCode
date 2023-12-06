using System.ComponentModel.DataAnnotations.Schema;
using YIT.__Domain.Entities.Bases;

namespace YIT.__Domain.Entities.Models._03Akaun
{
    public class AkCV : GenericTransactionFields
    {
        public int Id { get; set; }
        public string? NoRujukan { get; set; }
        public DateTime Tarikh { get; set; }
        [Column(TypeName = "decimal(18, 2)")]
        public decimal Jumlah { get; set; }
        public int AkPanjarId { get; set; }
        public AkPanjar? AkPanjar { get; set; }
        public ICollection<AkCVObjek>? AkCVObjek { get; set; }
    }
}