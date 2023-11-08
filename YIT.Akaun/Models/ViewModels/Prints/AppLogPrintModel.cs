using YIT.__Domain.Entities.Administrations;

namespace YIT.Akaun.Models.ViewModels.Prints
{
    public class AppLogPrintModel
    {
        public List<AppLog>? AppLog { get; set; }
        public CompanyDetails? CompanyDetail { get; set; }
    }
}
