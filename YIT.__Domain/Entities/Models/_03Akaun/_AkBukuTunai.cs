using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace YIT.__Domain.Entities.Models._03Akaun
{
    public class _AkBukuTunai
    {
        public DateTime? TarikhMasuk { get; set; }
        public string? NamaAkaunMasuk { get; set; }
        public string? NoRujukanMasuk { get; set; }
        public decimal AmaunMasuk { get; set; }
        public decimal JumlahMasuk { get; set; }
        public DateTime? TarikhKeluar { get; set; }
        public string? NamaAkaunKeluar { get; set; }
        public string? NoRujukanKeluar { get; set; }
        public decimal AmaunKeluar { get; set; }
        public decimal JumlahKeluar { get; set; }
        public int KeluarMasuk { get; set; }
    }
}
