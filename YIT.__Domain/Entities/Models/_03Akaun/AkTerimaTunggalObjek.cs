using System.ComponentModel.DataAnnotations.Schema;
using YIT.__Domain.Entities.Models._01Jadual;

namespace YIT.__Domain.Entities.Models._03Akaun
{
    public class AkTerimaTunggalObjek
    {
        public int Id { get; set; }
        public int AkTerimaTunggalId { get; set; }
        public AkTerimaTunggal? AkTerimaTunggal { get; set; }
        public int AkCartaId { get; set; }
        public AkCarta? AkCarta { get; set; }
        public int JKWPTJBahagianId { get; set; }
        public JKWPTJBahagian? JKWPTJBahagian { get; set; }
        [Column(TypeName = "decimal(18, 2)")]
        public decimal Amaun { get; set; }
    }
}