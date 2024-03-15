using YIT.__Domain.Entities._Enums;
using YIT.__Domain.Entities.Administrations;
using YIT.__Domain.Entities.Models._03Akaun;


namespace YIT.Akaun.Models.ViewModels.Prints
{
    public class LAK002PrintModel
    {
        public List<AkPV>? AkPV { get; set; }
        public List<AkJurnal>? AkJurnal { get; set; }
        public string? Username { get; set; }
        public string? KodLaporan { get; set; }
        public string? Tajuk1 { get; set; }
        public string? Tajuk2 { get; set; }

        public CompanyDetails? CompanyDetails { get; set; }
        

    }
}
