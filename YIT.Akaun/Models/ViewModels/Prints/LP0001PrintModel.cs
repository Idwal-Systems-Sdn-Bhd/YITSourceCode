using YIT.__Domain.Entities.Administrations;
using YIT.__Domain.Entities.Models._03Akaun;

namespace YIT.Akaun.Models.ViewModels.Prints
{
    public class LP0001PrintModel
    {
        public List<AkTerima>? AkTerima { get; set; }
        public string? Username { get; set; }
        public string? KodLaporan { get; set; }
        public string? Tajuk1 { get; set; }
        public string? Tajuk2 { get; set; }
        public CompanyDetails? CompanyDetails { get; set; }
    }
}