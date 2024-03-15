using YIT.__Domain.Entities._Enums;
using YIT.__Domain.Entities.Bases;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace YIT.__Domain.Entities.Models._03Akaun
{
    public class AkCarta : GenericFields
    {
        public int Id { get; set; }
        [Required(ErrorMessage = "Kod Akaun Diperlukan")]
        [DisplayName("Kod Akaun")]
        public string? Kod { get; set; }
        [Required(ErrorMessage = "Perihal Diperlukan")]
        [DisplayName("Perihal")]
        public string? Perihal { get; set; }
        [DisplayName("Debit/Kredit ")]
        public string? DebitKredit { get; set; }
        [DisplayName("Umum/Detail ")]
        public string? UmumDetail { get; set; }
        public string? Catatan { get; set; }
        [Column(TypeName = "decimal(18, 2)")]
        public decimal Baki { get; set; }
        [DisplayName("Jenis")]
        public EnJenisCarta EnJenis { get; set; }
        [DisplayName("Paras")]
        public EnParas EnParas { get; set; }
        public ICollection<AkAkaun>? AkAkaun1 { get; set; }
        public ICollection<AkAkaun>? AkAkaun2 { get; set; }
        public ICollection<AkTerimaObjek>? AkTerimaObjek { get; set; }
        public ICollection<AbBukuVot>? AbBukuVot { get; set; }
        public ICollection<AbWaranObjek>? AbWaranObjek { get; set; }
        public ICollection<AkPenilaianPerolehanObjek>? AkPenilaianPerolehanObjek { get; set; }
        public ICollection<AkNotaMintaObjek>? AkNotaMintaObjek { get; set; }
        public ICollection<AkIndenObjek>? AkIndenObjek { get; set; }
        public ICollection<AkPOObjek>? AkPOObjek { get; set; }
        public ICollection<AkPelarasanPOObjek>? AkPelarasanPOObjek { get; set; }
        public ICollection<AkPelarasanIndenObjek>? AkPelarasanIndenObjek { get; set; }
        public ICollection<AkBelianObjek>? AkBelianObjek { get; set; }
        public ICollection<AkPVObjek>? AkPVObjek { get; set; }
        public ICollection<AkCVObjek>? AkCVObjek { get; set; }
        public ICollection<AkNotaDebitKreditDiterimaObjek>? AkNotaDebitKreditDiterimaObjek { get; set; }
        public ICollection<AkJurnalObjek>? AkJurnalObjekDebit { get; set; }
        public ICollection<AkJurnalObjek>? AkJurnalObjekKredit { get; set; }
        public ICollection<AkPanjarLejar>? AkPanjarLejar { get; set; }

    }
}
