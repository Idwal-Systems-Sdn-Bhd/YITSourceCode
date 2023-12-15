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
    public class AkNotaDebitKreditDiterima : GenericTransactionFields
    {
        public int Id { get; set; }
        [DisplayName("Tahun Belanjawan")]
        public string? Tahun { get; set; }
        [DisplayName("No Rujukan")]
        public string? NoRujukan { get; set; }
        [DisplayName("Tarikh")]
        public DateTime Tarikh { get; set; }
        // nota :
        // 0 - debit
        // 1 - kredit
        public int FlDebitKredit { get; set; }
        public int AkBelianId { get; set; }
        public AkBelian? AkBelian { get; set; }
        [DisplayName("Jumlah RM")]
        [Column(TypeName = "decimal(18, 2)")]
        public decimal Jumlah { get; set; }
        public int JKWId { get; set; }
        public JKW? JKW { get; set; }
        public string? Ringkasan { get; set; }
        public ICollection<AkNotaDebitKreditDiterimaObjek>? AkNotaDebitKreditDiterimaObjek { get; set; }
        public ICollection<AkNotaDebitKreditDiterimaPerihal>? AkNotaDebitKreditDiterimaPerihal { get; set; }


    }
}
