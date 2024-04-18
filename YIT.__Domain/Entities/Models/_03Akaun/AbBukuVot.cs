using YIT.__Domain.Entities.Models._01Jadual;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using YIT.__Domain.Entities.Models._02Daftar;

namespace YIT.__Domain.Entities.Models._03Akaun
{
    public class AbBukuVot
    {
        public int Id { get; set; }
        public string? Tahun { get; set; }
        [DisplayName("KW")]
        public int JKWId { get; set; }
        public JKW? JKW { get; set; }
        
        [DisplayName("PTJ")]
        public int JPTJId { get; set; }
        public JPTJ? JPTJ { get; set; }
        [DisplayName("Bahagian")]
        public int? JBahagianId { get; set; }
        public JBahagian? JBahagian { get; set; }
        public DateTime Tarikh { get; set; }
        public int? DDaftarAwamId { get; set; }
        public DDaftarAwam? DDaftarAwam { get; set; }
        [DisplayName("Vot")]
        public int VotId { get; set; }
        public AkCarta? Vot { get; set; }
        public string? NoRujukan { get; set; }
        [DisplayName("Debit RM")]
        [Column(TypeName = "decimal(18, 2)")]
        public decimal Debit { get; set; }
        [DisplayName("Kredit RM")]
        [Column(TypeName = "decimal(18, 2)")]
        public decimal Kredit { get; set; }
        [DisplayName("Tanggungan RM")]
        [Column(TypeName = "decimal(18, 2)")]
        public decimal Tanggungan { get; set; }
        [DisplayName("TBS RM")]
        [Column(TypeName = "decimal(18, 2)")]
        public decimal Tbs { get; set; }
        [DisplayName("Baki RM")]
        [Column(TypeName = "decimal(18, 2)")]
        public decimal Baki { get; set; }
        [DisplayName("Rizab RM")]
        [Column(TypeName = "decimal(18, 2)")]
        public decimal Rizab { get; set; }
        [DisplayName("Liabiliti RM")]
        [Column(TypeName = "decimal(18, 2)")]
        public decimal Liabiliti { get; set; }
        [DisplayName("Belanja RM")]
        [Column(TypeName = "decimal(18, 2)")]
        public decimal Belanja { get; set; }
        [NotMapped]
        public decimal JumLiabiliti { get; set; }

    }
}
