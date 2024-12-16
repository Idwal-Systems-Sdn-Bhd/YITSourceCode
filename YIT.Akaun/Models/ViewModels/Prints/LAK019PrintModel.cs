using YIT.__Domain.Entities.Models._03Akaun;

namespace YIT.Akaun.Models.ViewModels.Prints
{
    public class LAK019PrintModel
    {
        public CommonPrintModel CommonModels { get; set; } = new CommonPrintModel();
        public List<AkEFT>? AkEFT { get; set; }
        public string? NamaDisedia { get; set; }
        public string? JawatanDisedia { get; set; }
        public string? NamaSemak { get; set; }
        public string? JawatanSemak { get; set; }
        public string? NamaDiluluskan { get; set; }
        public string? JawatanDiluluskan { get; set; }
    }
}