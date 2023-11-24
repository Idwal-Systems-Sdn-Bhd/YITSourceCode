using YIT.__Domain.Entities.Bases;
using YIT.__Domain.Entities.Models._01Jadual;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace YIT.__Domain.Entities.Models._03Akaun
{
    public class AkAkaun : GenericFields
    {
        public int Id { get; set; }
        public int JKWId { get; set; }
        public JKW? JKW { get; set; }
        public int? JPTJId { get; set; }
        public JPTJ? JPTJ { get; set; }
        public int? JBahagianId { get; set; }
        public JBahagian? JBahagian { get; set; }
        public int AkCarta1Id { get; set; }
        public AkCarta? AkCarta1 { get; set; }
        public int? AkCarta2Id { get; set; }
        public AkCarta? AkCarta2 { get; set; }
        public string? NoRujukan { get; set; }
        public DateTime Tarikh { get; set; }
        [Column(TypeName = "decimal(18, 2)")]
        public decimal Debit { get; set; }
        [Column(TypeName = "decimal(18, 2)")]
        public decimal Kredit { get; set; }
        public string? NoRujukanLama { get; set; } // dummy
    }
}
