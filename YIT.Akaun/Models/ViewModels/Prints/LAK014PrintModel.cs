using YIT.__Domain.Entities.Models._03Akaun;

namespace YIT.Akaun.Models.ViewModels.Prints
{
    public class LAK014PrintModel
    {
        public CommonPrintModel CommonModels { get; set; } = new CommonPrintModel();
        public List<AkPO>? AkPO { get; set; }
        public List<_AkPOResult>? AkPOResult { get; set; }
    }
}