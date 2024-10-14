using YIT.__Domain.Entities._Enums;

namespace YIT.__Domain.Entities.Models._03Akaun
{
    public class LAK005PrintModel
    {
        public string? Kod { get; set; }
        public string? Perihal { get; set; }
        public decimal? AnggaranHasil { get; set; }
        public decimal? HasilBulanan { get; set; }
        public decimal? HasilTerkumpul { get; set; }
        public decimal? BakiAnggaran { get; set; }
        public DateTime? Tarikh {  get; set; }
        public int AkAnggarId { get; set; }
        public AkAnggar? AkAnggar { get; set; }
        public int AkAnggarLejarId { get; set; }
        public AkAnggarLejar? AkAnggarLejar { get; set; }
    }
}
