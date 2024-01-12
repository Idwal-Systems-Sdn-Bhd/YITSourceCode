using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using YIT.__Domain.Entities._Enums;
using YIT.__Domain.Entities.Bases;
using YIT.__Domain.Entities.Models._01Jadual;

namespace YIT.__Domain.Entities.Models._03Akaun
{
    public class AkJurnal : GenericTransactionFields
    {
        public int Id { get; set; }
        public int JKWId { get; set; }
        public JKW? JKW { get; set; }
        [DisplayName("No Rujukan")]
        public string? NoRujukan { get; set; }
        public DateTime Tarikh { get; set; }
        [DisplayName("Jenis Jurnal")]
        public EnJenisJurnal EnJenisJurnal { get; set; }
        [Column(TypeName = "decimal(18, 2)")]
        [DisplayName("Jumlah Debit")]
        public decimal JumlahDebit { get; set; }
        [Column(TypeName = "decimal(18, 2)")]
        [DisplayName("Jumlah Kredit")]
        public decimal JumlahKredit { get; set; }
        public string? Ringkasan { get; set; }
        [DisplayName("No Baucer")]
        public int? AkPVId { get; set; }
        public AkPV? AkPV { get; set; }
        [DisplayName("Terlibat dengan Akaun Kena Bayar (AKB)?")]
        public bool IsAKB { get; set; }
        public ICollection<AkJurnalObjek>? AkJurnalObjek { get; set; }
        public ICollection<AkJurnalPenerimaCekBatal>? AkJurnalPenerimaCekBatal { get; set; }
    }
}
