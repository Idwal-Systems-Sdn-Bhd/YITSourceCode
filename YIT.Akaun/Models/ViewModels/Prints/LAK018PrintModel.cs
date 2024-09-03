using YIT.__Domain.Entities.Models._03Akaun;


namespace YIT.Akaun.Models.ViewModels.Prints
{
    public class LAK018PrintModel
    {
        public CommonPrintModel CommonModels { get; set; } = new CommonPrintModel();
        public List<AbWaran>? AbWaran { get; set; }
    }
}