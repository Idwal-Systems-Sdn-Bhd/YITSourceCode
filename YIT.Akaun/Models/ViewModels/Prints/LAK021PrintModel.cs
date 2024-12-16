using YIT.__Domain.Entities.Models._03Akaun;

namespace YIT.Akaun.Models.ViewModels.Prints
{
    public class LAK021PrintModel
    {
        public CommonPrintModel CommonModels { get; set; } = new CommonPrintModel();
        public List<AkPenilaianPerolehan>? AkPenilaianPerolehan { get; set; }
        public List<_AkPenilaianPerolehanResult>? AkPenilaianPerolehanResult { get; set; }
    }
}