using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using YIT.__Domain.Entities.Bases;

namespace YIT.__Domain.Entities.Models._03Akaun
{
    public class AkPenyesuaianBank : GenericTransactionFields
    {
        public int Id { get; set; }
        public string? Tahun { get; set; }
        public string? Bulan { get; set; }
        [DisplayName("No Rujukan")]
        public string? NoRujukan { get; set; }
        [DisplayName("Nama Fail")]
        public string? NamaFail { get; set; }
        public DateTime Tarikh { get; set; }
        [DisplayName("No Akaun Bank")]
        public int AkBankId { get; set; }
        public AkBank? AkBank { get; set; }
        [DisplayName("Muat naik")]
        public bool IsMuatNaik { get; set; }
        [DisplayName("Tarikh Dimuat naik")]
        public DateTime? TarikhMuatNaik { get; set; }
        [DisplayName("Kunci")]
        public bool IsKunci { get; set; }
        [DisplayName("Tarikh Dikunci")]
        public DateTime? TarikhKunci { get; set; }
        [DisplayName("Baki Penyata RM")]
        [Column(TypeName = "decimal(18, 2)")]
        public decimal BakiPenyata { get; set; }
        public string? NoRujukanLama { get; set; }
        public ICollection<AkPenyesuaianBankPenyataBank>? AkPenyesuaianBankPenyataBank { get; set; }
    }
}
