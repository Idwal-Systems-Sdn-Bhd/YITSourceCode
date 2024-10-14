using YIT.__Domain.Entities.Administrations;
using YIT.__Domain.Entities.Models._03Akaun;

namespace YIT.__Domain.Entities.Models._03Akaun
{
    public class LAK009PrintModel
    {
        public List<AkPV>? AkPV { get; set; }
        public DateTime? TarikhPV { get; set; }
        public string? NoRujukanPV { get; set; }
        public string? Perihal { get; set; }
        public decimal? Amaun { get; set; }
        public string? NoRujukanCek { get; set; }
        public string? PenerimaCek { get; set; }
        public string? akCartaKod {  get; set; }
        public string? akCartaPerihal { get; set; }
        public decimal? Jumlah { get; set; }
        public decimal? TotalJumlah { get; set; }

    }
}
