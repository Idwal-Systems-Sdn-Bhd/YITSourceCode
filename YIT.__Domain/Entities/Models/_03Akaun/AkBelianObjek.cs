using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using YIT.__Domain.Entities.Models._01Jadual;

namespace YIT.__Domain.Entities.Models._03Akaun
{
    public class AkBelianObjek 
    {
        public int Id { get; set; }
        public int AkBelianId { get; set; }
        public AkBelian? AkBelian { get; set; }
        [DisplayName("Carta")]
        public int AkCartaId { get; set; }
        public AkCarta? AkCarta { get; set; }
        [DisplayName("Bahagian")]
        public int JKWPTJBahagianId { get; set; }
        public JKWPTJBahagian? JKWPTJBahagian { get; set; }
        [DisplayName("Kod Cukai")]
        public int? JCukaiId { get; set; }
        public JCukai? JCukai { get; set; }
        [Column(TypeName = "decimal(18, 2)")]
        public decimal Amaun { get; set; }
    }
}
