using YIT.__Domain.Entities.Models._01Jadual;
using System.ComponentModel.DataAnnotations.Schema;

namespace YIT.__Domain.Entities.Models._03Akaun
{
    public class AbWaranObjek
    {
        public int Id { get; set; }
        public int AbWaranId { get; set; }
        public AbWaran? AbWaran { get; set; }
        public int AkCartaId { get; set; }
        public AkCarta? AkCarta { get; set; }
        public int JKWPTJBahagianId { get; set; }
        public JKWPTJBahagian? JKWPTJBahagian { get; set; }
        [Column(TypeName = "decimal(18, 2)")]
        public decimal Amaun { get; set; }
        public string? TK { get; set; }
    }
}