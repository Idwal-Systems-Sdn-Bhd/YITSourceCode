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
    public class AkPenyataPemungut : GenericTransactionFields
    {
        public int Id { get; set; }
        [DisplayName("Tahun Belanjawan")]
        public string? Tahun { get; set; }
        [DisplayName("No Rujukan")]
        public string? NoRujukan { get; set; }
        [DisplayName("No Slip")]
        public string? NoSlip { get; set; }
        public DateTime Tarikh { get; set; }
        [DisplayName("Cara Bayar")]
        public int JCaraBayarId { get; set; }
        public JCaraBayar? JCaraBayar { get; set; }
        [DisplayName("Akaun Bank")]
        public int AkBankId { get; set; }
        public AkBank? AkBank { get; set; }
        [DisplayName("Cawangan")]
        public int JCawanganId { get; set; }
        public JCawangan? JCawangan { get; set; }
        [DisplayName("PTJ")]
        public int JPTJId { get; set; }
        public JPTJ? JPTJ { get; set; }
        [DisplayName("Jenis Cek")]
        public EnJenisCek EnJenisCek { get; set; }
        [DisplayName("Bil Resit")]
        public int BilResit { get; set; }
        [Column(TypeName = "decimal(18, 2)")]
        public decimal Jumlah { get; set; }
        public string? NoRujukanLama { get; set; }
        public ICollection<AkPenyataPemungutObjek>? AkPenyataPemungutObjek { get; set; }
    }
}
