using System.ComponentModel.DataAnnotations.Schema;

namespace YIT.__Domain.Entities.Models._03Akaun
{
    public class AkPVInvois
    {
        public int Id { get; set; }
        public int AkPVId { get; set; }
        public AkPV? AkPV { get; set; }
        public bool IsTanggungan { get; set; }
        public int? AkPOId { get; set; }
        public AkPO? AkPO { get; set; }
        public int AkBelianId { get; set; }
        public AkBelian? AkBelian { get; set; }
        [Column(TypeName = "decimal(18, 2)")]
        public decimal Amaun { get; set; }
    }
}