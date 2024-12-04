using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using YIT.__Domain.Entities._Enums;

namespace YIT.__Domain.Entities.Models._03Akaun
{
    public class LAK004PrintModel
    {
        public string? Tahun {  get; set; }
        public string? Kod { get; set; }
        public string? Perihal { get; set; }
        public decimal? PeruntukanAsal { get; set; }
        public decimal? PeruntukanTambahan { get; set; }
        public decimal? PindahanPeruntukan { get; set; }
        public decimal? JumlahPeruntukan { get; set; }
        public decimal? PeruntukanTelahGuna { get; set; }
        public decimal? TBS { get; set; }
        public decimal? PerbelanjaanBersih { get; set; }
        public decimal? Baki { get; set; }
        public decimal? TelahGunaPercentage { get; set; }
        public decimal? TBSPercentage { get; set; }
        public decimal? BelanjaBersihPercentage { get; set; }
        public decimal? PerbelanjaanBulanIni { get; set; }
        public decimal? PerbelanjaanTerkumpul { get; set; }
        public decimal? BelanjaTerkumpulPercentage { get; set; }
        public decimal? BakiPercentage { get; set; }
        public EnJenisPeruntukan enJenisPeruntukan { get; set; }
        public List<AbWaranObjek>? AbWaranObjek { get; set; }
        public List<AbWaran>? AbWaran { get; set; }
    }
}
