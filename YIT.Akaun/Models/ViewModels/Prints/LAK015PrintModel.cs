using YIT.__Domain.Entities.Models._03Akaun;

namespace YIT.Akaun.Models.ViewModels.Prints
{
    public class LAK015PrintModel
    {
        public CommonPrintModel CommonModels { get; set; } = new CommonPrintModel();
        public List<_AkCartaResult>? AkCartaResult { get; set; }
        public decimal Jumlah1 { get; set; } 
        public decimal JumlahBaki { get; set; } 
        public decimal JumlahJan { get; set; }
        public decimal JumlahFeb { get; set; }
        public decimal JumlahMac { get; set; }
        public decimal JumlahApr { get; set; }
        public decimal JumlahMei { get; set; }
        public decimal JumlahJun { get; set; }
        public decimal JumlahJul { get; set; }
        public decimal JumlahOgo { get; set; }
        public decimal JumlahSep { get; set; }
        public decimal JumlahOkt { get; set; }
        public decimal JumlahNov { get; set; }
        public decimal JumlahDis { get; set; }
        public decimal JumlahAkhir { get; set; }
    }
}