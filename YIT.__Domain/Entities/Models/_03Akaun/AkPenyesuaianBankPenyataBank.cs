using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel;

namespace YIT.__Domain.Entities.Models._03Akaun
{
    public class AkPenyesuaianBankPenyataBank
    {
        public int Id { get; set; }
        public int AkPenyesuaianBankId { get; set; }
        public AkPenyesuaianBank? AkPenyesuaianBank { get; set; }
        [Column(TypeName = "decimal(18, 2)")]
        public decimal Bil { get; set; }
        public string? Indeks { get; set; }
        public string? NoAkaunBank { get; set; }
        public DateTime Tarikh { get; set; }
        public string? KodCawanganBank { get; set; }
        public string? KodTransaksi { get; set; }
        public string? PerihalTransaksi { get; set; }
        public string? NoDokumen { get; set; }
        public string? NoDokumenTambahan1 { get; set; }
        public string? NoDokumenTambahan2 { get; set; }
        public string? NoDokumenTambahan3 { get; set; }
        [Column(TypeName = "decimal(18, 2)")]
        public decimal Debit { get; set; }
        [Column(TypeName = "decimal(18, 2)")]
        public decimal Kredit { get; set; }
        public string? SignDebitKredit { get; set; }
        [Column(TypeName = "decimal(18, 2)")]
        public decimal Baki { get; set; }
        public bool IsPadan { get; set; }
        public ICollection<AkAkaunPenyataBank>? AkAkaunPenyataBank { get; set; }
    }
}