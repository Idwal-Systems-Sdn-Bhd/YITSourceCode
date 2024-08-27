using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using YIT.__Domain.Entities.Models._01Jadual;

namespace YIT.__Domain.Entities.Models._03Akaun
{
    public class AkAnggarLejar
    {
        public int Id { get; set; }
        public string? Tahun { get; set; }
        public string? NoRujukan { get; set; }
        public DateTime Tarikh { get; set; }
        public int JKWPTJBahagianId {  get; set; }
        public JKWPTJBahagian? JKWPTJBahagian {  get; set; }
        public int AkCartaId { get; set; }
        public AkCarta? AkCarta { get; set; }
        public decimal Amaun { get; set; }
        public decimal Sebenar { get; set; }
        public decimal Baki { get; set; }
        public int AkAnggarId { get; set; }
        public AkAnggar? AkAnggar { get; set; }
    }
}
