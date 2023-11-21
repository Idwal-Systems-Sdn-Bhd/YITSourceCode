using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using YIT.__Domain.Entities.Bases;
using YIT.__Domain.Entities.Models._01Jadual;

namespace YIT.__Domain.Entities.Models._03Akaun
{
    public class AkPelarasanPO : GenericTransactionFields
    {
        public int Id { get; set; }
        public string? Tahun { get; set; }
        [DisplayName("No Rujukan")]
        public string? NoRujukan { get; set; }
        public DateTime Tarikh { get; set; }
        [DisplayName("No Pesanan Tempatan")]
        public int AkPOId { get; set; }
        public AkPO? AkPO { get; set; }
        [DisplayName("Jumlah RM")]
        [Column(TypeName = "decimal(18, 2)")]
        public decimal Jumlah { get; set; }
        [DisplayName("Kump. Wang")]
        public int JKWId { get; set; }
        public JKW? JKW { get; set; }
        public string? Ringkasan { get; set; }
        public ICollection<AkPelarasanPOObjek>? AkPelarasanPOObjek { get; set; }
        public ICollection<AkPelarasanPOPerihal>? AkPelarasanPOPerihal { get; set; }
    }
}
