using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel;
using YIT.__Domain.Entities.Models._01Jadual;

namespace YIT.__Domain.Entities.Models._03Akaun
{
    public class AkPOObjek
    {
        public int Id { get; set; }
        public int AkPOId { get; set; }
        public AkPO? AkPO { get; set; }
        [DisplayName("Carta")]
        public int AkCartaId { get; set; }
        public AkCarta? AkCarta { get; set; }
        [DisplayName("Bahagian")]
        public int JBahagianId { get; set; }
        public JBahagian? JBahagian { get; set; }
        [Column(TypeName = "decimal(18, 2)")]
        public decimal Amaun { get; set; }
    }
}