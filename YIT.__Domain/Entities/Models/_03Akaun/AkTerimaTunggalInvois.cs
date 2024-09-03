using System.ComponentModel.DataAnnotations.Schema;

namespace YIT.__Domain.Entities.Models._03Akaun
{
    public class AkTerimaTunggalInvois
    {
        public int Id { get; set; }
        public int AkTerimaTunggalId { get; set; }
        public AkTerimaTunggal? AkTerimaTunggal { get; set; }
        public int AkInvoisId { get; set; }
        public AkInvois? AkInvois { get; set; }
        [Column(TypeName = "decimal(18, 2)")]
        public decimal Amaun { get; set; }
    }
}