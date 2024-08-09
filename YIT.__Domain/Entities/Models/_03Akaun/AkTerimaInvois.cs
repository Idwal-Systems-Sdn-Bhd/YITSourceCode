using System.ComponentModel.DataAnnotations.Schema;

namespace YIT.__Domain.Entities.Models._03Akaun
{
    public class AkTerimaInvois
    {
        public int Id { get; set; }
        public int AkTerimaId { get; set; }
        public AkTerima? AkTerima { get; set; }
        public bool IsTanggungan { get; set; }
        public int AkBelianId { get; set; }
        public int AkInvoisId { get; set; }
        public AkInvois? AkInvois { get; set; }
        [Column(TypeName = "decimal(18, 2)")]
        public decimal Amaun { get; set; }
    }
}