using System.ComponentModel.DataAnnotations.Schema;
using YIT.__Domain.Entities.Bases;
using YIT.__Domain.Entities.Models._02Daftar;

namespace YIT.__Domain.Entities.Models._03Akaun
{
    public class AkCV : GenericTransactionFields
    {
        public int Id { get; set; }
        public string? NoRujukan { get; set; }
        public DateTime Tarikh { get; set; }
        [Column(TypeName = "decimal(18, 2)")]
        public decimal Jumlah { get; set; }
        public int DPanjarId { get; set; }
        public DPanjar? DPanjar { get; set; }
        public ICollection<AkCVObjek>? AkCVObjek { get; set; }
    }
}