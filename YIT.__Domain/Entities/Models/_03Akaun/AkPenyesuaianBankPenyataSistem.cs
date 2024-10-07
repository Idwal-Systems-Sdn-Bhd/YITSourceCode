using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using YIT.__Domain.Entities.Models._01Jadual;

namespace YIT.__Domain.Entities.Models._03Akaun
{
    public class AkPenyesuaianBankPenyataSistem
    {
        public int Id { get; set; } // akPVPenerimaId / akTerimaTunggalId / akJurnalId
        public int? AkAkaunId { get; set; }
        public DateTime Tarikh { get; set; }
        public string? NoRujukan { get; set; }
        public string? Perihal { get; set; }
        public string? NoSlip { get; set; }
        public decimal Debit { get; set; }
        public decimal Kredit { get; set; }
        public bool IsPV { get; set; }
        public int? JCarabayarId { get; set; }
        public JCaraBayar? JCarabayar { get; set; }
        public bool IsPadan { get; set; }
    }
}
