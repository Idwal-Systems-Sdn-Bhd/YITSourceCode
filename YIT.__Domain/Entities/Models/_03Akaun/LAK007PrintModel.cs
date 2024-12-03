using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace YIT.__Domain.Entities.Models._03Akaun
{
    public class LAK007PrintModel
    {
        public List<AkAkaun>? akAkaun { get; set; }
        public string? KodAkaun { get; set; }
        public string? NamaAkaun { get; set; }
        public decimal? Jumlah { get; set; }
        public string? Jenis { get; set; }
        public string? Tahun { get; set; }
        public string? Bulan { get; set; }
        public decimal? TahunLps_BulanSMS { get; set; }
        public decimal? TahunSms_BulanSMS { get; set; }
        public decimal? TahunLps_Kumpul { get; set; }
        public decimal? TahunSms_Kumpul { get; set; }
        public decimal? TerKumpulPercentage { get; set; }
        public decimal? PeruntukanPercentage { get; set; }
        public decimal? TahunSms_Peruntukan { get; set; }
        public string? Paras { get; set; }
    }
}
