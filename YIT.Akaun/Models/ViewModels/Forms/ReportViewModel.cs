using YIT.__Domain.Entities.Models._03Akaun;

namespace YIT.Akaun.Models.ViewModels.Forms
{
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