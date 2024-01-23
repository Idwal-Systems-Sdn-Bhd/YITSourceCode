using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace YIT.__Domain.Entities.Models._03Akaun
{
    public class _AkTimbangDuga
    {
        public string? KodAkaun { get; set; }
        public string? NamaAkaun { get; set; }
        public int JKWId { get; set; }
        public int? JPTJId {  get; set; }
        public string? DebitKredit { get; set; }
        public string? Jenis { get; set; }
        public decimal Debit { get; set; }
        public decimal Kredit { get; set; }
    }
}
