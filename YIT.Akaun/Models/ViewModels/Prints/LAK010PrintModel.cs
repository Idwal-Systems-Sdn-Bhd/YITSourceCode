using YIT.__Domain.Entities.Models._03Akaun;

namespace YIT.Akaun.Models.ViewModels.Prints
{
    public class LAK010PrintModel
    {
        public CommonPrintModel CommonModels { get; set; } = new CommonPrintModel();
        public List<AkBelian>? AkBelian { get; set; }
        public List<AkBelianWithTunggakan>? AkBelianWithTunggakanList { get; set; }
        public decimal Jumlah { get; set; }
    }

    public class AkBelianWithTunggakan
    {
        public AkBelian? AkBelian { get; set; }
        public int? TunggakanHari { get; set; }
        public string? TTerimaKewangan { get; set; }
        public string? akBPerihal { get; set; }
        public string? FormattedTAkuanKewangan { get; set; }
        public string? TarikhPVInvois { get; set; }
        public string? KodBelianObjek { get; set; }
        public string? NoRujukanPVInvois { get; set; }
        public string? NoRujukan { get; set; }
        public string? Batal { get; set; }
    }
}