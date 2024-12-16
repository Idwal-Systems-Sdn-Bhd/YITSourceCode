using YIT.__Domain.Entities.Models._03Akaun;

namespace YIT.Akaun.Models.ViewModels.Prints
{
    public class LAK002PrintModel
    {
        public CommonPrintModel CommonModels { get; set; } = new CommonPrintModel();
        public List<AkPV>? AkPV { get; set; }
        public List<AkJurnal>? AkJurnal { get; set; }
        public ReportViewModel ReportViewModel { get; set; } = new ReportViewModel();
    }
    public class ReportViewModel
    {
        public string? NamaSyarikat { get; set; }
        public string? AlamatSyarikat1 { get; set; }
        public string? AlamatSyarikat2 { get; set; }
        public string? AlamatSyarikat3 { get; set; }
        public string? TarikhDari { get; set; }
        public string? TarikhHingga { get; set; }
        public string? Tunai { get; set; }
        public List<GroupedReportModel> GroupedReportModel { get; set; } = new List<GroupedReportModel>();
    }

    public class GroupedReportModel
    {
        public string? AkBank { get; set; }
        public List<AkPVPenerima>? AkPVPenerima { get; set; }
        public AkCarta? AkCarta1 { get; set; }
    }
}