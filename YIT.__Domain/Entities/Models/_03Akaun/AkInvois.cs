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
using YIT.__Domain.Entities.Models._02Daftar;
using YIT.__Domain.Entities.Models._50LHDN;

namespace YIT.__Domain.Entities.Models._03Akaun
{
    public class AkInvois : GenericTransactionFields
    {
        public int Id { get; set; }
        [DisplayName("Tahun Belanjawan")]
        public string? Tahun { get; set; }
        [DisplayName("No Rujukan")]
        public string? NoRujukan { get; set; }
        [DisplayName("Tarikh")]
        public DateTime Tarikh { get; set; }

        [DisplayName("Pelanggan")]
        public int DDaftarAwamId { get; set; }
        public DDaftarAwam? DDaftarAwam { get; set; }
        [DisplayName("Akaun Penghutang")]
        public int? AkAkaunAkruId { get; set; }
        public AkCarta? AkAkaunAkru { get; set; }
        [DisplayName("Jumlah RM")]
        [Column(TypeName = "decimal(18, 2)")]
        public decimal Jumlah { get; set; }
        [DisplayName("Kump. Wang")]
        public int JKWId { get; set; }
        public JKW? JKW { get; set; }
        public string? Ringkasan { get; set; }
        [DisplayName("No e-Invois")]
        public int? LHDNEInvoisId { get; set; }
        public LHDNEInvois? LHDNEInvois { get; set; }
        [DisplayName("MSIC")]
        public int? LHDNMSICId { get; set; }
        public LHDNMSIC? LHDNMSIC { get; set; }
        public string? UUID { get; set; }
        [Column(TypeName = "decimal(18, 2)")]
        public decimal JumlahCukai { get; set; }
        [Column(TypeName = "decimal(18, 2)")]
        public decimal JumlahTanpaCukai { get; set; }
        public string? KodPbklLama { get; set; }
        public ICollection<AkInvoisObjek>? AkInvoisObjek { get; set; }
        public ICollection<AkInvoisPerihal>? AkInvoisPerihal { get; set; }

    }
}
