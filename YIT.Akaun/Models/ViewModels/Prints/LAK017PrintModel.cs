using YIT.__Domain.Entities.Models._03Akaun;

namespace YIT.Akaun.Models.ViewModels.Prints
{
    public class LAK017PrintModel
    {
        public CommonPrintModel CommonModels { get; set; } = new CommonPrintModel();
        public List<AkJurnal>? AkJurnal { get; set; }
        public List<AkPO>? AkPO { get; set; }
        public List<_AkJurnalResult>? AkJurnalResult { get; set; }
        public List<_AkPOResult>? AkPOResult { get;set; }
    }
}