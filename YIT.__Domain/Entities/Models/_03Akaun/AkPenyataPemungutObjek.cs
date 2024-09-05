using System.ComponentModel.DataAnnotations.Schema;
using YIT.__Domain.Entities.Models._01Jadual;

namespace YIT.__Domain.Entities.Models._03Akaun
{
    public class AkPenyataPemungutObjek
    {
        public int Id { get; set; }
        public int AkPenyataPemungutId { get; set; }
        public AkPenyataPemungut? AkPenyataPemungut { get; set; }
        public int AkTerimaTunggalId { get; set; }
        public AkTerimaTunggal? AkTerimaTunggal { get; set; }
        [Column(TypeName = "decimal(18, 2)")]
        public decimal Bil { get; set; }
        public int JKWPTJBahagianId { get; set; }
        public JKWPTJBahagian? JKWPTJBahagian { get; set; }
        public int AkCartaId { get; set; }
        public AkCarta? AkCarta { get; set; }
        
        [Column(TypeName = "decimal(18, 2)")]
        public decimal Amaun { get; set; }
    }
}